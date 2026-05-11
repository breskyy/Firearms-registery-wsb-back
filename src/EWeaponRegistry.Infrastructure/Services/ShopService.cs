using EWeaponRegistry.Application.DTOs.Shop;
using EWeaponRegistry.Application.Exceptions;
using EWeaponRegistry.Application.Interfaces;
using EWeaponRegistry.Domain.Entities;
using EWeaponRegistry.Domain.Enums;
using EWeaponRegistry.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EWeaponRegistry.Infrastructure.Services;

public class ShopService : IShopService
{
    private readonly AppDbContext _context;
    private readonly IEncryptionService _encryptionService;
    private readonly IAuditService _auditService;

    public ShopService(AppDbContext context, IEncryptionService encryptionService, IAuditService auditService)
    {
        _context = context;
        _encryptionService = encryptionService;
        _auditService = auditService;
    }

    public async Task<VerifyPermitResponse> VerifyPermitAsync(Guid shopUserId, VerifyPermitRequest request)
    {
        // Verify shop is valid
        var shop = await _context.Shops.FirstOrDefaultAsync(s => s.UserId == shopUserId);
        if (shop == null || !shop.IsVerified)
        {
            await _auditService.LogAsync("ShopVerifyPermit.ShopNotVerified", "Shop", shopUserId.ToString());
            throw new ForbiddenException("Shop is not verified");
        }

        // Find promise by QR token or promise number
        Promise? promise = null;

        if (!string.IsNullOrEmpty(request.QrToken))
        {
            promise = await _context.Promises
                .Include(p => p.Citizen)
                .Include(p => p.Permit)
                .FirstOrDefaultAsync(p => p.QrToken == request.QrToken);
        }
        else if (!string.IsNullOrEmpty(request.PromiseNumber))
        {
            promise = await _context.Promises
                .Include(p => p.Citizen)
                .Include(p => p.Permit)
                .FirstOrDefaultAsync(p => p.PromiseNumber == request.PromiseNumber);
        }

        if (promise == null)
        {
            await _auditService.LogAsync("ShopVerifyPermit.PromiseNotFound", "Promise",
                request.QrToken ?? request.PromiseNumber);
            return new VerifyPermitResponse
            {
                IsValid = false,
                Message = "Promise not found"
            };
        }

        // Validate promise status
        if (promise.Status != PromiseStatus.Active && promise.Status != PromiseStatus.Approved)
        {
            return new VerifyPermitResponse
            {
                IsValid = false,
                Message = $"Promise is not active (status: {promise.Status})"
            };
        }

        // Validate expiry
        if (promise.ExpiryDate < DateTime.UtcNow.Date)
        {
            return new VerifyPermitResponse
            {
                IsValid = false,
                Message = "Promise has expired"
            };
        }

        // Validate remaining quantity
        if (promise.UsedQuantity >= promise.Quantity)
        {
            return new VerifyPermitResponse
            {
                IsValid = false,
                Message = "Promise has been fully used"
            };
        }

        // Validate permit
        var permit = promise.Permit;
        if (permit.Status != PermitStatus.Active || permit.ExpiryDate < DateTime.UtcNow.Date)
        {
            return new VerifyPermitResponse
            {
                IsValid = false,
                Message = "Associated permit is not valid"
            };
        }

        // Check medical exams
        var medicalExpiry = _encryptionService.DecryptDate(permit.MedicalExamExpiryDateEncrypted);
        var psychExpiry = _encryptionService.DecryptDate(permit.PsychologicalExamExpiryDateEncrypted);
        var medicalExamsValid = medicalExpiry >= DateTime.UtcNow.Date && psychExpiry >= DateTime.UtcNow.Date;

        await _auditService.LogAsync("ShopVerifyPermit.Success", "Promise", promise.Id.ToString(),
            newValues: new { promise.PromiseNumber, CitizenId = promise.CitizenId });

        return new VerifyPermitResponse
        {
            IsValid = true,
            Message = "Verification successful",
            CitizenName = $"{_encryptionService.Decrypt(promise.Citizen.FirstNameEncrypted)} {_encryptionService.Decrypt(promise.Citizen.LastNameEncrypted)}",
            PermitNumber = permit.PermitNumber,
            PermitType = permit.PermitType.ToString(),
            AvailableSlots = permit.MaxFirearms - permit.UsedSlots,
            WeaponType = promise.WeaponType,
            RemainingPromiseQuantity = promise.Quantity - promise.UsedQuantity,
            PromiseExpiryDate = promise.ExpiryDate,
            MedicalExamsValid = medicalExamsValid
        };
    }

    public async Task<RegisterSaleResponse> RegisterSaleAsync(Guid shopUserId, RegisterSaleRequest request)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // Verify shop
            var shop = await _context.Shops.FirstOrDefaultAsync(s => s.UserId == shopUserId);
            if (shop == null || !shop.IsVerified)
            {
                await _auditService.LogAsync("ShopRegisterSale.ShopNotVerified", "Shop", shopUserId.ToString());
                throw new ForbiddenException("Shop is not verified");
            }

            // Find and validate promise
            var promise = await _context.Promises
                .Include(p => p.Citizen)
                .Include(p => p.Permit)
                .FirstOrDefaultAsync(p => p.QrToken == request.QrToken);

            if (promise == null)
                throw new NotFoundException("Promise with provided QR token not found");

            // Business rule validations
            if (promise.Status != PromiseStatus.Active && promise.Status != PromiseStatus.Approved)
                throw new BusinessRuleViolationException($"Promise is not active (status: {promise.Status})");

            if (promise.ExpiryDate < DateTime.UtcNow.Date)
                throw new BusinessRuleViolationException("Promise has expired");

            if (promise.UsedQuantity >= promise.Quantity)
                throw new BusinessRuleViolationException("Promise has been fully used");

            var permit = promise.Permit;
            if (permit.Status != PermitStatus.Active)
                throw new BusinessRuleViolationException("Permit is not active");

            if (permit.ExpiryDate < DateTime.UtcNow.Date)
                throw new BusinessRuleViolationException("Permit has expired");

            // Check permit slots
            if (permit.UsedSlots >= permit.MaxFirearms)
                throw new BusinessRuleViolationException("Permit has reached maximum firearms limit");

            // Validate category matches permit type
            if (!IsPermitValidForCategory(permit.PermitType, request.Category))
            {
                await _auditService.LogAsync("ShopRegisterSale.CategoryMismatch", "Firearm", null,
                    newValues: new { permit.PermitType, request.Category },
                    description: $"Category {request.Category} not allowed for permit type {permit.PermitType}");
                throw new BusinessRuleViolationException(
                    $"Category {request.Category} is not allowed for permit type {permit.PermitType}");
            }

            // Check medical exams
            var medicalExpiry = _encryptionService.DecryptDate(permit.MedicalExamExpiryDateEncrypted);
            var psychExpiry = _encryptionService.DecryptDate(permit.PsychologicalExamExpiryDateEncrypted);

            if (medicalExpiry < DateTime.UtcNow.Date)
                throw new BusinessRuleViolationException("Medical exam has expired");

            if (psychExpiry < DateTime.UtcNow.Date)
                throw new BusinessRuleViolationException("Psychological exam has expired");

            // Check serial number uniqueness
            var existingFirearm = await _context.Firearms
                .FirstOrDefaultAsync(f => f.SerialNumber == request.SerialNumber &&
                                          f.Status != FirearmStatus.Archived);

            if (existingFirearm != null)
            {
                await _auditService.LogAsync("ShopRegisterSale.DuplicateSerialNumber", "Firearm", existingFirearm.Id.ToString(),
                    newValues: new { request.SerialNumber },
                    description: "Attempted to register firearm with duplicate serial number");
                throw new ConflictException($"Firearm with serial number {request.SerialNumber} already exists");
            }

            // Create firearm
            var firearm = new Firearm
            {
                Id = Guid.NewGuid(),
                OwnerCitizenId = promise.CitizenId,
                Brand = request.Brand,
                Model = request.Model,
                Category = request.Category,
                Caliber = request.Caliber,
                SerialNumber = request.SerialNumber,
                ProductionYear = request.ProductionYear,
                Status = FirearmStatus.Registered,
                RegisteredAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            _context.Firearms.Add(firearm);

            // Create ownership history
            var history = new OwnershipHistory
            {
                Id = Guid.NewGuid(),
                FirearmId = firearm.Id,
                PreviousOwnerCitizenId = null, // First registration - no previous owner
                NewOwnerCitizenId = promise.CitizenId,
                TransferType = TransferType.Sale,
                TransferDate = DateTime.UtcNow,
                CreatedByUserId = shopUserId,
                CreatedAt = DateTime.UtcNow,
                Notes = $"Purchased from {shop.Name}"
            };

            _context.OwnershipHistories.Add(history);

            // Update promise
            promise.UsedQuantity++;
            if (promise.UsedQuantity >= promise.Quantity)
            {
                promise.Status = PromiseStatus.Used;
            }

            // Update permit
            permit.UsedSlots++;
            permit.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            await _auditService.LogAsync("ShopRegisterSale.Success", "Firearm", firearm.Id.ToString(),
                newValues: new
                {
                    firearm.SerialNumber,
                    firearm.Brand,
                    firearm.Model,
                    firearm.Category,
                    CitizenId = promise.CitizenId,
                    ShopId = shop.Id,
                    PromiseId = promise.Id
                });

            return new RegisterSaleResponse
            {
                Success = true,
                Message = "Firearm registered successfully",
                FirearmId = firearm.Id,
                RegistrationNumber = firearm.SerialNumber
            };
        }
        catch (AppException)
        {
            await transaction.RollbackAsync();
            throw;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            await _auditService.LogAsync("ShopRegisterSale.Error", "Firearm", null,
                description: ex.Message);
            throw;
        }
    }

    private static bool IsPermitValidForCategory(PermitType permitType, FirearmCategory category)
    {
        return permitType switch
        {
            PermitType.Sport => category is FirearmCategory.A or FirearmCategory.B,
            PermitType.Collection => category is FirearmCategory.A or FirearmCategory.B or FirearmCategory.C,
            PermitType.Protection => category == FirearmCategory.B,
            PermitType.Hunting => category == FirearmCategory.C,
            PermitType.Other => true,
            _ => false
        };
    }
}
