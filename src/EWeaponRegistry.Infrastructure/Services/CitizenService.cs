using EWeaponRegistry.Application.DTOs.Citizen;
using EWeaponRegistry.Application.Exceptions;
using EWeaponRegistry.Application.Interfaces;
using EWeaponRegistry.Domain.Entities;
using EWeaponRegistry.Domain.Enums;
using EWeaponRegistry.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EWeaponRegistry.Infrastructure.Services;

public class CitizenService : ICitizenService
{
    private readonly AppDbContext _context;
    private readonly IEncryptionService _encryptionService;
    private readonly IAuditService _auditService;

    public CitizenService(AppDbContext context, IEncryptionService encryptionService, IAuditService auditService)
    {
        _context = context;
        _encryptionService = encryptionService;
        _auditService = auditService;
    }

    public async Task<CitizenProfileDto> GetMyProfileAsync(Guid userId)
    {
        var citizen = await _context.CitizenProfiles
            .FirstOrDefaultAsync(c => c.UserId == userId)
            ?? throw new NotFoundException("Citizen profile not found for current user");

        var pesel = _encryptionService.Decrypt(citizen.PeselEncrypted);

        await _auditService.LogAsync("ViewOwnProfile", "CitizenProfile", citizen.Id.ToString());

        return new CitizenProfileDto
        {
            Id = citizen.Id,
            FirstName = _encryptionService.Decrypt(citizen.FirstNameEncrypted),
            LastName = _encryptionService.Decrypt(citizen.LastNameEncrypted),
            PeselMasked = $"*******{pesel[^4..]}",
            Address = _encryptionService.Decrypt(citizen.AddressEncrypted),
            DocumentNumber = _encryptionService.Decrypt(citizen.DocumentNumberEncrypted),
            WeaponBookNumber = _encryptionService.Decrypt(citizen.WeaponBookNumberEncrypted),
            CreatedAt = citizen.CreatedAt
        };
    }

    public async Task<IList<PermitDto>> GetMyPermitsAsync(Guid userId)
    {
        var citizen = await _context.CitizenProfiles
            .Include(c => c.Permits)
            .FirstOrDefaultAsync(c => c.UserId == userId)
            ?? throw new NotFoundException("Citizen profile not found");

        return citizen.Permits.Select(p => new PermitDto
        {
            Id = p.Id,
            PermitNumber = p.PermitNumber,
            PermitType = p.PermitType,
            Status = p.Status,
            IssueDate = p.IssueDate,
            ExpiryDate = p.ExpiryDate,
            MaxFirearms = p.MaxFirearms,
            UsedSlots = p.UsedSlots,
            MedicalExamExpiryDate = _encryptionService.DecryptDate(p.MedicalExamExpiryDateEncrypted),
            PsychologicalExamExpiryDate = _encryptionService.DecryptDate(p.PsychologicalExamExpiryDateEncrypted)
        }).ToList();
    }

    public async Task<IList<FirearmDto>> GetMyFirearmsAsync(Guid userId)
    {
        var citizen = await _context.CitizenProfiles
            .Include(c => c.Firearms)
            .FirstOrDefaultAsync(c => c.UserId == userId)
            ?? throw new NotFoundException("Citizen profile not found");

        return citizen.Firearms
            .Where(f => f.Status == FirearmStatus.Registered)
            .Select(f => new FirearmDto
            {
                Id = f.Id,
                Brand = f.Brand,
                Model = f.Model,
                Category = f.Category,
                Caliber = f.Caliber,
                SerialNumber = f.SerialNumber,
                ProductionYear = f.ProductionYear,
                Status = f.Status,
                RegisteredAt = f.RegisteredAt
            }).ToList();
    }

    public async Task<FirearmDetailDto?> GetMyFirearmByIdAsync(Guid userId, Guid firearmId)
    {
        var citizen = await _context.CitizenProfiles
            .FirstOrDefaultAsync(c => c.UserId == userId)
            ?? throw new NotFoundException("Citizen profile not found");

        var firearm = await _context.Firearms
            .Include(f => f.OwnershipHistories)
            .ThenInclude(oh => oh.NewOwner)
            .FirstOrDefaultAsync(f => f.Id == firearmId && f.OwnerCitizenId == citizen.Id);

        if (firearm == null)
            return null;

        await _auditService.LogAsync("ViewFirearmDetails", "Firearm", firearmId.ToString());

        return new FirearmDetailDto
        {
            Id = firearm.Id,
            Brand = firearm.Brand,
            Model = firearm.Model,
            Category = firearm.Category,
            Caliber = firearm.Caliber,
            SerialNumber = firearm.SerialNumber,
            ProductionYear = firearm.ProductionYear,
            Status = firearm.Status,
            RegisteredAt = firearm.RegisteredAt,
            OwnershipHistory = firearm.OwnershipHistories
                .OrderByDescending(oh => oh.TransferDate)
                .Select(oh => new OwnershipHistoryDto
                {
                    Id = oh.Id,
                    PreviousOwnerName = oh.PreviousOwner != null
                        ? $"{_encryptionService.Decrypt(oh.PreviousOwner.FirstNameEncrypted)} {_encryptionService.Decrypt(oh.PreviousOwner.LastNameEncrypted)}"
                        : null,
                    NewOwnerName = $"{_encryptionService.Decrypt(oh.NewOwner.FirstNameEncrypted)} {_encryptionService.Decrypt(oh.NewOwner.LastNameEncrypted)}",
                    TransferType = oh.TransferType,
                    TransferDate = oh.TransferDate,
                    Notes = oh.Notes
                }).ToList()
        };
    }

    public async Task<IList<PromiseDto>> GetMyPromisesAsync(Guid userId)
    {
        var citizen = await _context.CitizenProfiles
            .Include(c => c.Promises)
            .FirstOrDefaultAsync(c => c.UserId == userId)
            ?? throw new NotFoundException("Citizen profile not found");

        return citizen.Promises.Select(p => new PromiseDto
        {
            Id = p.Id,
            PromiseNumber = p.PromiseNumber,
            WeaponType = p.WeaponType,
            Quantity = p.Quantity,
            UsedQuantity = p.UsedQuantity,
            Status = p.Status,
            FeeAmount = p.FeeAmount,
            PaymentStatus = p.PaymentStatus,
            QrToken = p.QrToken,
            IssueDate = p.IssueDate,
            ExpiryDate = p.ExpiryDate
        }).ToList();
    }

    public async Task<IList<PromiseApplicationDto>> GetMyPromiseApplicationsAsync(Guid userId)
    {
        var citizen = await _context.CitizenProfiles
            .Include(c => c.PromiseApplications)
            .ThenInclude(pa => pa.Permit)
            .FirstOrDefaultAsync(c => c.UserId == userId)
            ?? throw new NotFoundException("Citizen profile not found");

        return citizen.PromiseApplications.Select(pa => new PromiseApplicationDto
        {
            Id = pa.Id,
            PermitId = pa.PermitId,
            PermitNumber = pa.Permit.PermitNumber,
            RequestedWeaponType = pa.RequestedWeaponType,
            RequestedQuantity = pa.RequestedQuantity,
            Status = pa.Status,
            RejectionReason = pa.RejectionReason,
            CorrectionNotes = pa.CorrectionNotes,
            CreatedAt = pa.CreatedAt,
            ReviewedAt = pa.ReviewedAt
        }).ToList();
    }

    public async Task<PromiseApplicationDto> CreatePromiseApplicationAsync(Guid userId, CreatePromiseApplicationRequest request)
    {
        var citizen = await _context.CitizenProfiles
            .Include(c => c.Permits)
            .FirstOrDefaultAsync(c => c.UserId == userId)
            ?? throw new NotFoundException("Citizen profile not found");

        var permit = citizen.Permits.FirstOrDefault(p => p.Id == request.PermitId)
            ?? throw new NotFoundException("Permit", request.PermitId);

        // Validate permit is active and not expired
        if (permit.Status != PermitStatus.Active)
            throw new BusinessRuleViolationException("Permit is not active");

        if (permit.ExpiryDate < DateTime.UtcNow.Date)
            throw new BusinessRuleViolationException("Permit has expired");

        // Check available slots
        if (permit.UsedSlots + request.RequestedQuantity > permit.MaxFirearms)
            throw new BusinessRuleViolationException(
                $"Requested quantity ({request.RequestedQuantity}) exceeds available slots ({permit.MaxFirearms - permit.UsedSlots})");

        var application = new PromiseApplication
        {
            Id = Guid.NewGuid(),
            CitizenId = citizen.Id,
            PermitId = permit.Id,
            RequestedWeaponType = request.RequestedWeaponType,
            RequestedQuantity = request.RequestedQuantity,
            Status = PromiseApplicationStatus.Submitted,
            CreatedAt = DateTime.UtcNow
        };

        _context.PromiseApplications.Add(application);
        await _context.SaveChangesAsync();

        await _auditService.LogAsync("CreatePromiseApplication", "PromiseApplication", application.Id.ToString(),
            newValues: new { request.PermitId, request.RequestedWeaponType, request.RequestedQuantity });

        return new PromiseApplicationDto
        {
            Id = application.Id,
            PermitId = application.PermitId,
            PermitNumber = permit.PermitNumber,
            RequestedWeaponType = application.RequestedWeaponType,
            RequestedQuantity = application.RequestedQuantity,
            Status = application.Status,
            CreatedAt = application.CreatedAt
        };
    }

    public async Task<IList<TransferRequestDto>> GetMyTransferRequestsAsync(Guid userId)
    {
        var citizen = await _context.CitizenProfiles
            .FirstOrDefaultAsync(c => c.UserId == userId)
            ?? throw new NotFoundException("Citizen profile not found");

        var transfers = await _context.TransferRequests
            .Include(tr => tr.Firearm)
            .Include(tr => tr.Buyer)
            .Include(tr => tr.Seller)
            .Where(tr => tr.SellerCitizenId == citizen.Id || tr.BuyerCitizenId == citizen.Id)
            .ToListAsync();

        return transfers.Select(tr => new TransferRequestDto
        {
            Id = tr.Id,
            FirearmId = tr.FirearmId,
            FirearmDescription = $"{tr.Firearm.Brand} {tr.Firearm.Model} ({tr.Firearm.SerialNumber})",
            BuyerName = tr.Buyer != null
                ? $"{_encryptionService.Decrypt(tr.Buyer.FirstNameEncrypted)} {_encryptionService.Decrypt(tr.Buyer.LastNameEncrypted)}"
                : null,
            TransferType = tr.TransferType,
            Status = tr.Status,
            TransactionDate = tr.TransactionDate,
            CreatedAt = tr.CreatedAt,
            IsSeller = tr.SellerCitizenId == citizen.Id,
            IsBuyer = tr.BuyerCitizenId == citizen.Id
        }).ToList();
    }

    public async Task<TransferRequestDto> CreateTransferRequestAsync(Guid userId, CreateTransferRequest request)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var seller = await _context.CitizenProfiles
                .Include(c => c.Firearms)
                .FirstOrDefaultAsync(c => c.UserId == userId)
                ?? throw new NotFoundException("Citizen profile not found");

            var firearm = seller.Firearms.FirstOrDefault(f => f.Id == request.FirearmId)
                ?? throw new NotFoundException("Firearm", request.FirearmId);

            if (firearm.Status != FirearmStatus.Registered)
                throw new BusinessRuleViolationException("Firearm is not in registered status and cannot be transferred");

            // Find buyer by PESEL
            var buyerPeselEncrypted = _encryptionService.Encrypt(request.BuyerPesel);
            var allCitizens = await _context.CitizenProfiles.ToListAsync();
            var buyer = allCitizens.FirstOrDefault(c =>
                _encryptionService.Decrypt(c.PeselEncrypted) == request.BuyerPesel);

            if (buyer == null)
                throw new NotFoundException("Buyer with provided PESEL not found in the system");

            if (buyer.Id == seller.Id)
                throw new BusinessRuleViolationException("Cannot transfer firearm to yourself");

            var transferRequest = new TransferRequest
            {
                Id = Guid.NewGuid(),
                FirearmId = firearm.Id,
                SellerCitizenId = seller.Id,
                BuyerCitizenId = buyer.Id,
                BuyerPeselEncrypted = buyerPeselEncrypted,
                TransferType = request.TransferType,
                Status = TransferRequestStatus.PendingAcceptance,
                CreatedAt = DateTime.UtcNow
            };

            _context.TransferRequests.Add(transferRequest);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            await _auditService.LogAsync("CreateTransferRequest", "TransferRequest", transferRequest.Id.ToString(),
                newValues: new { firearm.Id, firearm.SerialNumber, BuyerId = buyer.Id, request.TransferType });

            return new TransferRequestDto
            {
                Id = transferRequest.Id,
                FirearmId = firearm.Id,
                FirearmDescription = $"{firearm.Brand} {firearm.Model} ({firearm.SerialNumber})",
                BuyerName = $"{_encryptionService.Decrypt(buyer.FirstNameEncrypted)} {_encryptionService.Decrypt(buyer.LastNameEncrypted)}",
                TransferType = transferRequest.TransferType,
                Status = transferRequest.Status,
                CreatedAt = transferRequest.CreatedAt,
                IsSeller = true,
                IsBuyer = false
            };
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task AcceptTransferRequestAsync(Guid userId, Guid transferRequestId)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var buyer = await _context.CitizenProfiles
                .Include(c => c.Permits)
                .FirstOrDefaultAsync(c => c.UserId == userId)
                ?? throw new NotFoundException("Citizen profile not found");

            var transferRequest = await _context.TransferRequests
                .Include(tr => tr.Firearm)
                .FirstOrDefaultAsync(tr => tr.Id == transferRequestId)
                ?? throw new NotFoundException("Transfer request", transferRequestId);

            if (transferRequest.BuyerCitizenId != buyer.Id)
                throw new ForbiddenException("You are not the buyer in this transfer request");

            if (transferRequest.Status != TransferRequestStatus.PendingAcceptance)
                throw new BusinessRuleViolationException("Transfer request is not pending acceptance");

            // Validate buyer has valid permit
            var validPermit = buyer.Permits.FirstOrDefault(p =>
                p.Status == PermitStatus.Active &&
                p.ExpiryDate >= DateTime.UtcNow.Date &&
                p.UsedSlots < p.MaxFirearms &&
                IsPermitValidForCategory(p.PermitType, transferRequest.Firearm.Category));

            if (validPermit == null)
                throw new BusinessRuleViolationException("Buyer does not have a valid permit for this firearm category");

            // Check medical exams
            var medicalExpiry = _encryptionService.DecryptDate(validPermit.MedicalExamExpiryDateEncrypted);
            var psychExpiry = _encryptionService.DecryptDate(validPermit.PsychologicalExamExpiryDateEncrypted);

            if (medicalExpiry < DateTime.UtcNow.Date || psychExpiry < DateTime.UtcNow.Date)
                throw new BusinessRuleViolationException("Buyer's medical or psychological exam has expired");

            // Update ownership
            var oldOwnerId = transferRequest.Firearm.OwnerCitizenId;
            transferRequest.Firearm.OwnerCitizenId = buyer.Id;
            transferRequest.Firearm.UpdatedAt = DateTime.UtcNow;

            // Create ownership history
            var history = new OwnershipHistory
            {
                Id = Guid.NewGuid(),
                FirearmId = transferRequest.FirearmId,
                PreviousOwnerCitizenId = transferRequest.SellerCitizenId,
                NewOwnerCitizenId = buyer.Id,
                TransferType = transferRequest.TransferType,
                TransferDate = DateTime.UtcNow,
                CreatedByUserId = userId,
                CreatedAt = DateTime.UtcNow,
                Notes = "Transfer accepted by buyer"
            };

            _context.OwnershipHistories.Add(history);

            // Update permit slots
            validPermit.UsedSlots++;

            // Update seller's permit slots
            var sellerPermit = await _context.Permits
                .Where(p => p.CitizenId == transferRequest.SellerCitizenId && p.Status == PermitStatus.Active)
                .FirstOrDefaultAsync();
            if (sellerPermit != null)
            {
                sellerPermit.UsedSlots = Math.Max(0, sellerPermit.UsedSlots - 1);
            }

            transferRequest.Status = TransferRequestStatus.Completed;
            transferRequest.TransactionDate = DateTime.UtcNow;
            transferRequest.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            await _auditService.LogAsync("AcceptTransfer", "TransferRequest", transferRequestId.ToString(),
                oldValues: new { OwnerId = oldOwnerId },
                newValues: new { OwnerId = buyer.Id, transferRequest.Firearm.SerialNumber });
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task RejectTransferRequestAsync(Guid userId, Guid transferRequestId)
    {
        var buyer = await _context.CitizenProfiles
            .FirstOrDefaultAsync(c => c.UserId == userId)
            ?? throw new NotFoundException("Citizen profile not found");

        var transferRequest = await _context.TransferRequests
            .FirstOrDefaultAsync(tr => tr.Id == transferRequestId)
            ?? throw new NotFoundException("Transfer request", transferRequestId);

        if (transferRequest.BuyerCitizenId != buyer.Id)
            throw new ForbiddenException("You are not the buyer in this transfer request");

        if (transferRequest.Status != TransferRequestStatus.PendingAcceptance)
            throw new BusinessRuleViolationException("Transfer request is not pending acceptance");

        transferRequest.Status = TransferRequestStatus.Rejected;
        transferRequest.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        await _auditService.LogAsync("RejectTransfer", "TransferRequest", transferRequestId.ToString());
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
