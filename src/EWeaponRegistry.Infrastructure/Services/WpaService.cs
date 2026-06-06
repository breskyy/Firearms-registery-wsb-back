using EWeaponRegistry.Application.DTOs.Citizen;
using EWeaponRegistry.Application.DTOs.Common;
using EWeaponRegistry.Application.DTOs.Wpa;
using EWeaponRegistry.Application.Exceptions;
using EWeaponRegistry.Application.Interfaces;
using EWeaponRegistry.Application.Interfaces.ExternalGateways;
using EWeaponRegistry.Domain.Entities;
using EWeaponRegistry.Domain.Enums;
using EWeaponRegistry.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EWeaponRegistry.Infrastructure.Services;

public class WpaService : IWpaService
{
    private readonly AppDbContext _context;
    private readonly IEncryptionService _encryptionService;
    private readonly IAuditService _auditService;
    private readonly IMObywatelGateway _mObywatelGateway;

    public WpaService(
        AppDbContext context,
        IEncryptionService encryptionService,
        IAuditService auditService,
        IMObywatelGateway mObywatelGateway)
    {
        _context = context;
        _encryptionService = encryptionService;
        _auditService = auditService;
        _mObywatelGateway = mObywatelGateway;
    }

    public async Task<PaginatedResult<WpaCitizenDto>> GetCitizensAsync(PaginationParams pagination)
    {
        var query = _context.CitizenProfiles
            .Include(c => c.Permits)
            .Include(c => c.Firearms)
            .Include(c => c.MedicalAlerts)
            .AsQueryable();

        var totalCount = await query.CountAsync();

        var citizens = await query
            .OrderBy(c => c.CreatedAt)
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToListAsync();

        var items = citizens.Select(c => new WpaCitizenDto
        {
            Id = c.Id,
            UserId = c.UserId,
            FirstName = _encryptionService.Decrypt(c.FirstNameEncrypted),
            LastName = _encryptionService.Decrypt(c.LastNameEncrypted),
            Pesel = _encryptionService.Decrypt(c.PeselEncrypted),
            Address = _encryptionService.Decrypt(c.AddressEncrypted),
            DocumentNumber = _encryptionService.Decrypt(c.DocumentNumberEncrypted),
            WeaponBookNumber = _encryptionService.Decrypt(c.WeaponBookNumberEncrypted),
            CreatedAt = c.CreatedAt,
            Permits = c.Permits.Select(p => new PermitDto
            {
                Id = p.Id,
                PermitNumber = p.PermitNumber,
                PermitType = p.PermitType,
                Status = p.Status,
                IssueDate = p.IssueDate,
                ExpiryDate = p.ExpiryDate,
                MaxFirearms = p.MaxFirearms,
                UsedSlots = p.UsedSlots
            }).ToList(),
            TotalFirearms = c.Firearms.Count(f => f.Status == FirearmStatus.Registered),
            ActiveAlerts = c.MedicalAlerts.Count(a => !a.IsResolved)
        }).ToList();

        await _auditService.LogAsync("WpaViewCitizens", "CitizenProfile", null,
            description: $"Viewed page {pagination.Page} of citizens list");

        return new PaginatedResult<WpaCitizenDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = pagination.Page,
            PageSize = pagination.PageSize
        };
    }

    public async Task<WpaCitizenDto?> GetCitizenByIdAsync(Guid citizenId)
    {
        var citizen = await _context.CitizenProfiles
            .Include(c => c.Permits)
            .Include(c => c.Firearms)
            .Include(c => c.MedicalAlerts)
            .FirstOrDefaultAsync(c => c.Id == citizenId);

        if (citizen == null)
            return null;

        await _auditService.LogAsync("WpaViewCitizenDetails", "CitizenProfile", citizenId.ToString(),
            description: "Viewed citizen personal details");

        return new WpaCitizenDto
        {
            Id = citizen.Id,
            UserId = citizen.UserId,
            FirstName = _encryptionService.Decrypt(citizen.FirstNameEncrypted),
            LastName = _encryptionService.Decrypt(citizen.LastNameEncrypted),
            Pesel = _encryptionService.Decrypt(citizen.PeselEncrypted),
            Address = _encryptionService.Decrypt(citizen.AddressEncrypted),
            DocumentNumber = _encryptionService.Decrypt(citizen.DocumentNumberEncrypted),
            WeaponBookNumber = _encryptionService.Decrypt(citizen.WeaponBookNumberEncrypted),
            CreatedAt = citizen.CreatedAt,
            Permits = citizen.Permits.Select(p => new PermitDto
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
            }).ToList(),
            TotalFirearms = citizen.Firearms.Count(f => f.Status == FirearmStatus.Registered),
            ActiveAlerts = citizen.MedicalAlerts.Count(a => !a.IsResolved)
        };
    }

    public async Task<PaginatedResult<WpaFirearmSearchResult>> SearchFirearmsAsync(
        string? serialNumber,
        string? pesel,
        string? permitNumber,
        PermitType? permitType,
        PaginationParams pagination)
    {
        var query = _context.Firearms
            .Include(f => f.Owner)
            .ThenInclude(o => o.Permits)
            .AsQueryable();

        if (!string.IsNullOrEmpty(serialNumber))
        {
            query = query.Where(f => f.SerialNumber.Contains(serialNumber));
        }

        if (permitType.HasValue)
        {
            query = query.Where(f => f.Owner.Permits.Any(p => p.PermitType == permitType.Value));
        }

        var firearms = await query.ToListAsync();

        // Filter by PESEL (need to decrypt)
        if (!string.IsNullOrEmpty(pesel))
        {
            firearms = firearms.Where(f =>
                _encryptionService.Decrypt(f.Owner.PeselEncrypted).Contains(pesel)).ToList();
        }

        // Filter by permit number
        if (!string.IsNullOrEmpty(permitNumber))
        {
            firearms = firearms.Where(f =>
                f.Owner.Permits.Any(p => p.PermitNumber.Contains(permitNumber))).ToList();
        }

        var totalCount = firearms.Count;

        var items = firearms
            .OrderByDescending(f => f.RegisteredAt)
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .Select(f =>
            {
                var permit = f.Owner.Permits.FirstOrDefault(p => p.Status == PermitStatus.Active)
                             ?? f.Owner.Permits.FirstOrDefault();

                return new WpaFirearmSearchResult
                {
                    Id = f.Id,
                    Brand = f.Brand,
                    Model = f.Model,
                    Category = f.Category.ToString(),
                    Caliber = f.Caliber,
                    SerialNumber = f.SerialNumber,
                    Status = f.Status.ToString(),
                    OwnerName = $"{_encryptionService.Decrypt(f.Owner.FirstNameEncrypted)} {_encryptionService.Decrypt(f.Owner.LastNameEncrypted)}",
                    OwnerPesel = _encryptionService.Decrypt(f.Owner.PeselEncrypted),
                    PermitNumber = permit?.PermitNumber ?? "N/A",
                    PermitType = permit?.PermitType.ToString() ?? "N/A",
                    RegisteredAt = f.RegisteredAt
                };
            }).ToList();

        await _auditService.LogAsync("WpaSearchFirearms", "Firearm", null,
            newValues: new { serialNumber, pesel = !string.IsNullOrEmpty(pesel), permitNumber, permitType },
            description: $"Searched firearms, found {totalCount} results");

        return new PaginatedResult<WpaFirearmSearchResult>
        {
            Items = items,
            TotalCount = totalCount,
            Page = pagination.Page,
            PageSize = pagination.PageSize
        };
    }

    public async Task<PaginatedResult<WpaPromiseApplicationDto>> GetPromiseApplicationsAsync(
        PromiseApplicationStatus? status,
        PaginationParams pagination)
    {
        var query = _context.PromiseApplications
            .Include(pa => pa.Citizen)
            .Include(pa => pa.Permit)
            .Include(pa => pa.ReviewedByOfficer)
            .Include(pa => pa.Attachments)
            .AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(pa => pa.Status == status.Value);
        }

        var totalCount = await query.CountAsync();

        var applications = await query
            .OrderByDescending(pa => pa.CreatedAt)
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToListAsync();

        var items = applications.Select(MapToWpaPromiseApplicationDto).ToList();

        return new PaginatedResult<WpaPromiseApplicationDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = pagination.Page,
            PageSize = pagination.PageSize
        };
    }

    public async Task<WpaPromiseApplicationDto?> GetPromiseApplicationByIdAsync(Guid applicationId)
    {
        var pa = await _context.PromiseApplications
            .Include(p => p.Citizen)
            .Include(p => p.Permit)
            .Include(p => p.ReviewedByOfficer)
            .Include(p => p.Attachments)
            .FirstOrDefaultAsync(p => p.Id == applicationId);

        if (pa == null)
            return null;

        await _auditService.LogAsync("WpaViewPromiseApplication", "PromiseApplication", applicationId.ToString());

        return MapToWpaPromiseApplicationDto(pa);
    }

    public async Task MarkApplicationUnderReviewAsync(Guid officerId, Guid applicationId)
    {
        var application = await _context.PromiseApplications
            .FirstOrDefaultAsync(pa => pa.Id == applicationId)
            ?? throw new NotFoundException("Promise application", applicationId);

        if (application.Status != PromiseApplicationStatus.Submitted &&
            application.Status != PromiseApplicationStatus.Paid)
        {
            throw new BusinessRuleViolationException(
                $"Cannot mark application as under review (current status: {application.Status})");
        }

        if (application.PaymentStatus != PaymentStatus.Paid)
            throw new BusinessRuleViolationException(
                $"Cannot mark application as under review until payment is verified (current payment status: {application.PaymentStatus})");

        var oldStatus = application.Status;
        application.Status = PromiseApplicationStatus.UnderReview;
        application.ReviewedByOfficerId = officerId;
        application.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        await _auditService.LogAsync("WpaMarkUnderReview", "PromiseApplication", applicationId.ToString(),
            oldValues: new { Status = oldStatus },
            newValues: new { Status = application.Status });
    }

    public async Task ApproveApplicationAsync(Guid officerId, Guid applicationId)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var application = await _context.PromiseApplications
                .Include(pa => pa.Permit)
                .Include(pa => pa.Citizen)
                .FirstOrDefaultAsync(pa => pa.Id == applicationId)
                ?? throw new NotFoundException("Promise application", applicationId);

            if (application.Status != PromiseApplicationStatus.UnderReview &&
                application.Status != PromiseApplicationStatus.Paid)
            {
                throw new BusinessRuleViolationException(
                    $"Cannot approve application (current status: {application.Status})");
            }

            if (application.PaymentStatus != PaymentStatus.Paid)
                throw new BusinessRuleViolationException(
                    $"Cannot approve application until payment is verified (current payment status: {application.PaymentStatus})");

            var oldStatus = application.Status;

            // Create promise
            var promise = new Promise
            {
                Id = Guid.NewGuid(),
                CitizenId = application.CitizenId,
                PermitId = application.PermitId,
                PromiseNumber = $"PROM-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}",
                WeaponType = application.RequestedWeaponType,
                Quantity = application.RequestedQuantity,
                UsedQuantity = 0,
                Status = PromiseStatus.Active,
                FeeAmount = application.FeeAmount,
                PaymentStatus = PaymentStatus.Paid,
                IssueDate = DateTime.UtcNow,
                ExpiryDate = DateTime.UtcNow.AddMonths(3),
                CreatedAt = DateTime.UtcNow
            };

            // Generate QR token (mock)
            var qrResult = await _mObywatelGateway.GenerateQrTokenAsync(promise.PromiseNumber, application.CitizenId);
            if (qrResult.Success)
            {
                promise.QrToken = qrResult.QrToken;
            }

            _context.Promises.Add(promise);

            application.Status = PromiseApplicationStatus.Approved;
            application.ReviewedByOfficerId = officerId;
            application.ReviewedAt = DateTime.UtcNow;
            application.GeneratedPromiseId = promise.Id;
            application.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            await _auditService.LogAsync("WpaApproveApplication", "PromiseApplication", applicationId.ToString(),
                oldValues: new { Status = oldStatus },
                newValues: new { Status = application.Status, PromiseId = promise.Id, promise.PromiseNumber });
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task RejectApplicationAsync(Guid officerId, Guid applicationId, string? reason)
    {
        var application = await _context.PromiseApplications
            .FirstOrDefaultAsync(pa => pa.Id == applicationId)
            ?? throw new NotFoundException("Promise application", applicationId);

        if (application.Status == PromiseApplicationStatus.Approved ||
            application.Status == PromiseApplicationStatus.Rejected)
        {
            throw new BusinessRuleViolationException(
                $"Cannot reject application (current status: {application.Status})");
        }

        var oldStatus = application.Status;
        application.Status = PromiseApplicationStatus.Rejected;
        application.RejectionReason = reason;
        application.ReviewedByOfficerId = officerId;
        application.ReviewedAt = DateTime.UtcNow;
        application.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        await _auditService.LogAsync("WpaRejectApplication", "PromiseApplication", applicationId.ToString(),
            oldValues: new { Status = oldStatus },
            newValues: new { Status = application.Status, reason });
    }

    public async Task RequireCorrectionAsync(Guid officerId, Guid applicationId, string? notes)
    {
        var application = await _context.PromiseApplications
            .FirstOrDefaultAsync(pa => pa.Id == applicationId)
            ?? throw new NotFoundException("Promise application", applicationId);

        if (application.Status != PromiseApplicationStatus.UnderReview &&
            application.Status != PromiseApplicationStatus.Submitted)
        {
            throw new BusinessRuleViolationException(
                $"Cannot require correction for application (current status: {application.Status})");
        }

        var oldStatus = application.Status;
        application.Status = PromiseApplicationStatus.RequiresCorrection;
        application.CorrectionNotes = notes;
        application.ReviewedByOfficerId = officerId;
        application.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        await _auditService.LogAsync("WpaRequireCorrection", "PromiseApplication", applicationId.ToString(),
            oldValues: new { Status = oldStatus },
            newValues: new { Status = application.Status, notes });
    }

    public async Task<PaginatedResult<MedicalAlertDto>> GetMedicalAlertsAsync(bool? resolved, PaginationParams pagination)
    {
        var query = _context.MedicalAlerts
            .Include(ma => ma.Citizen)
            .Include(ma => ma.Permit)
            .AsQueryable();

        if (resolved.HasValue)
        {
            query = query.Where(ma => ma.IsResolved == resolved.Value);
        }

        var totalCount = await query.CountAsync();

        var alerts = await query
            .OrderByDescending(ma => ma.CreatedAt)
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToListAsync();

        var items = alerts.Select(ma => new MedicalAlertDto
        {
            Id = ma.Id,
            CitizenId = ma.CitizenId,
            CitizenName = $"{_encryptionService.Decrypt(ma.Citizen.FirstNameEncrypted)} {_encryptionService.Decrypt(ma.Citizen.LastNameEncrypted)}",
            CitizenPesel = _encryptionService.Decrypt(ma.Citizen.PeselEncrypted),
            PermitId = ma.PermitId,
            PermitNumber = ma.Permit?.PermitNumber,
            AlertType = ma.AlertType,
            Message = ma.Message,
            DueDate = ma.DueDate,
            IsResolved = ma.IsResolved,
            CreatedAt = ma.CreatedAt
        }).ToList();

        return new PaginatedResult<MedicalAlertDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = pagination.Page,
            PageSize = pagination.PageSize
        };
    }

    public async Task<PaginatedResult<WpaPermitApplicationDto>> GetPermitApplicationsAsync(
        PermitApplicationStatus? status,
        PaginationParams pagination)
    {
        var query = _context.PermitApplications
            .Include(pa => pa.Citizen)
            .Include(pa => pa.ReviewedByOfficer)
            .Include(pa => pa.Attachments)
            .AsQueryable();

        if (status.HasValue)
            query = query.Where(pa => pa.Status == status.Value);

        var totalCount = await query.CountAsync();

        var applications = await query
            .OrderByDescending(pa => pa.CreatedAt)
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToListAsync();

        var items = applications.Select(pa => MapToWpaPermitApplicationDto(pa)).ToList();

        return new PaginatedResult<WpaPermitApplicationDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = pagination.Page,
            PageSize = pagination.PageSize
        };
    }

    public async Task<WpaPermitApplicationDto?> GetPermitApplicationByIdAsync(Guid applicationId)
    {
        var pa = await _context.PermitApplications
            .Include(p => p.Citizen)
            .Include(p => p.ReviewedByOfficer)
            .Include(p => p.Attachments)
            .FirstOrDefaultAsync(p => p.Id == applicationId);

        if (pa == null)
            return null;

        await _auditService.LogAsync("WpaViewPermitApplication", "PermitApplication", applicationId.ToString());

        return MapToWpaPermitApplicationDto(pa);
    }

    public async Task MarkPermitApplicationUnderReviewAsync(Guid officerId, Guid applicationId)
    {
        var application = await _context.PermitApplications
            .FirstOrDefaultAsync(pa => pa.Id == applicationId)
            ?? throw new NotFoundException("Permit application", applicationId);

        if (application.Status != PermitApplicationStatus.Submitted)
            throw new BusinessRuleViolationException(
                $"Cannot mark application as under review (current status: {application.Status})");

        if (application.PaymentStatus != PaymentStatus.Paid)
            throw new BusinessRuleViolationException(
                $"Cannot mark application as under review until payment is verified (current payment status: {application.PaymentStatus})");

        var oldStatus = application.Status;
        application.Status = PermitApplicationStatus.UnderReview;
        application.ReviewedByOfficerId = officerId;
        application.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        await _auditService.LogAsync("WpaMarkPermitUnderReview", "PermitApplication", applicationId.ToString(),
            oldValues: new { Status = oldStatus },
            newValues: new { Status = application.Status });
    }

    public async Task ApprovePermitApplicationAsync(Guid officerId, Guid applicationId, ApprovePermitApplicationRequest request)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var application = await _context.PermitApplications
                .Include(pa => pa.Citizen)
                .Include(pa => pa.Attachments)
                .FirstOrDefaultAsync(pa => pa.Id == applicationId)
                ?? throw new NotFoundException("Permit application", applicationId);

            if (application.Status != PermitApplicationStatus.UnderReview &&
                application.Status != PermitApplicationStatus.Submitted)
                throw new BusinessRuleViolationException(
                    $"Cannot approve application (current status: {application.Status})");

            if (application.PaymentStatus != PaymentStatus.Paid)
                throw new BusinessRuleViolationException(
                    $"Cannot approve application until payment is verified (current payment status: {application.PaymentStatus})");

            var oldStatus = application.Status;

            if (request.MedicalExamExpiryDate == null || request.PsychologicalExamExpiryDate == null)
                throw new BusinessRuleViolationException("Medical and psychological exam expiry dates are required after document verification");

            if (!application.Attachments.Any(a => a.AttachmentType == PermitApplicationAttachmentType.MedicalCertificate) ||
                !application.Attachments.Any(a => a.AttachmentType == PermitApplicationAttachmentType.PsychologicalCertificate))
                throw new BusinessRuleViolationException("Both medical and psychological certificates must be attached before approval");

            var medicalExpiry = request.MedicalExamExpiryDate.Value;
            var psychExpiry = request.PsychologicalExamExpiryDate.Value;

            var permit = new Permit
            {
                Id = Guid.NewGuid(),
                CitizenId = application.CitizenId,
                PermitNumber = $"POZW-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}",
                PermitType = application.RequestedPermitType,
                Status = PermitStatus.Active,
                IssueDate = DateTime.UtcNow,
                ExpiryDate = DateTime.UtcNow.AddYears(10),
                MaxFirearms = request.MaxFirearms,
                UsedSlots = 0,
                MedicalExamExpiryDateEncrypted = _encryptionService.EncryptDate(medicalExpiry),
                PsychologicalExamExpiryDateEncrypted = _encryptionService.EncryptDate(psychExpiry),
                CreatedAt = DateTime.UtcNow
            };

            _context.Permits.Add(permit);

            application.Status = PermitApplicationStatus.Approved;
            application.ReviewedByOfficerId = officerId;
            application.ReviewedAt = DateTime.UtcNow;
            application.GeneratedPermitId = permit.Id;
            application.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            await _auditService.LogAsync("WpaApprovePermitApplication", "PermitApplication", applicationId.ToString(),
                oldValues: new { Status = oldStatus },
                newValues: new { Status = application.Status, PermitId = permit.Id, permit.PermitNumber });
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task RejectPermitApplicationAsync(Guid officerId, Guid applicationId, string? reason)
    {
        var application = await _context.PermitApplications
            .FirstOrDefaultAsync(pa => pa.Id == applicationId)
            ?? throw new NotFoundException("Permit application", applicationId);

        if (application.Status == PermitApplicationStatus.Approved ||
            application.Status == PermitApplicationStatus.Rejected)
            throw new BusinessRuleViolationException(
                $"Cannot reject application (current status: {application.Status})");

        var oldStatus = application.Status;
        application.Status = PermitApplicationStatus.Rejected;
        application.RejectionReason = reason;
        application.ReviewedByOfficerId = officerId;
        application.ReviewedAt = DateTime.UtcNow;
        application.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        await _auditService.LogAsync("WpaRejectPermitApplication", "PermitApplication", applicationId.ToString(),
            oldValues: new { Status = oldStatus },
            newValues: new { Status = application.Status, reason });
    }

    public async Task RequirePermitApplicationCorrectionAsync(Guid officerId, Guid applicationId, string? notes)
    {
        var application = await _context.PermitApplications
            .FirstOrDefaultAsync(pa => pa.Id == applicationId)
            ?? throw new NotFoundException("Permit application", applicationId);

        if (application.Status != PermitApplicationStatus.UnderReview &&
            application.Status != PermitApplicationStatus.Submitted)
            throw new BusinessRuleViolationException(
                $"Cannot require correction for application (current status: {application.Status})");

        var oldStatus = application.Status;
        application.Status = PermitApplicationStatus.RequiresCorrection;
        application.CorrectionNotes = notes;
        application.ReviewedByOfficerId = officerId;
        application.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        await _auditService.LogAsync("WpaRequirePermitCorrection", "PermitApplication", applicationId.ToString(),
            oldValues: new { Status = oldStatus },
            newValues: new { Status = application.Status, notes });
    }

    public async Task SuspendPermitAsync(Guid officerId, Guid permitId, string? reason)
    {
        var permit = await _context.Permits
            .FirstOrDefaultAsync(p => p.Id == permitId)
            ?? throw new NotFoundException("Permit", permitId);

        if (permit.Status != PermitStatus.Active)
            throw new BusinessRuleViolationException($"Cannot suspend permit (current status: {permit.Status})");

        var oldStatus = permit.Status;
        permit.Status = PermitStatus.Suspended;
        permit.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        await _auditService.LogAsync("WpaSuspendPermit", "Permit", permitId.ToString(),
            oldValues: new { Status = oldStatus },
            newValues: new { Status = permit.Status, reason });
    }

    public async Task RevokePermitAsync(Guid officerId, Guid permitId, string? reason)
    {
        var permit = await _context.Permits
            .FirstOrDefaultAsync(p => p.Id == permitId)
            ?? throw new NotFoundException("Permit", permitId);

        if (permit.Status == PermitStatus.Revoked)
            throw new BusinessRuleViolationException("Permit is already revoked");

        var oldStatus = permit.Status;
        permit.Status = PermitStatus.Revoked;
        permit.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        await _auditService.LogAsync("WpaRevokePermit", "Permit", permitId.ToString(),
            oldValues: new { Status = oldStatus },
            newValues: new { Status = permit.Status, reason });
    }

    public async Task RestorePermitAsync(Guid officerId, Guid permitId, string? reason)
    {
        var permit = await _context.Permits
            .FirstOrDefaultAsync(p => p.Id == permitId)
            ?? throw new NotFoundException("Permit", permitId);

        if (permit.Status != PermitStatus.Suspended)
            throw new BusinessRuleViolationException($"Cannot restore permit (current status: {permit.Status})");

        var oldStatus = permit.Status;
        permit.Status = PermitStatus.Active;
        permit.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        await _auditService.LogAsync("WpaRestorePermit", "Permit", permitId.ToString(),
            oldValues: new { Status = oldStatus },
            newValues: new { Status = permit.Status, reason });
    }

    public async Task UpdatePermitMedicalExamsAsync(Guid officerId, Guid permitId, UpdatePermitMedicalExamsRequest request)
    {
        var permit = await _context.Permits
            .FirstOrDefaultAsync(p => p.Id == permitId)
            ?? throw new NotFoundException("Permit", permitId);

        permit.MedicalExamExpiryDateEncrypted = _encryptionService.EncryptDate(request.MedicalExamExpiryDate);
        permit.PsychologicalExamExpiryDateEncrypted = _encryptionService.EncryptDate(request.PsychologicalExamExpiryDate);
        permit.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        await _auditService.LogAsync("WpaUpdateMedicalExams", "Permit", permitId.ToString(),
            newValues: new { request.MedicalExamExpiryDate, request.PsychologicalExamExpiryDate });
    }

    public async Task VerifyPermitApplicationPaymentAsync(Guid officerId, Guid applicationId)
    {
        var application = await _context.PermitApplications
            .FirstOrDefaultAsync(pa => pa.Id == applicationId)
            ?? throw new NotFoundException("Permit application", applicationId);

        if (application.PaymentStatus != PaymentStatus.Submitted)
            throw new BusinessRuleViolationException(
                $"Cannot verify payment (current payment status: {application.PaymentStatus})");

        var oldPaymentStatus = application.PaymentStatus;
        application.PaymentStatus = PaymentStatus.Paid;
        application.ReviewedByOfficerId = officerId;
        application.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        await _auditService.LogAsync("WpaVerifyPermitPayment", "PermitApplication", applicationId.ToString(),
            oldValues: new { PaymentStatus = oldPaymentStatus },
            newValues: new { PaymentStatus = application.PaymentStatus });
    }

    public async Task VerifyPromiseApplicationPaymentAsync(Guid officerId, Guid applicationId)
    {
        var application = await _context.PromiseApplications
            .FirstOrDefaultAsync(pa => pa.Id == applicationId)
            ?? throw new NotFoundException("Promise application", applicationId);

        if (application.PaymentStatus != PaymentStatus.Submitted)
            throw new BusinessRuleViolationException(
                $"Cannot verify payment (current payment status: {application.PaymentStatus})");

        var oldPaymentStatus = application.PaymentStatus;
        var oldStatus = application.Status;
        application.PaymentStatus = PaymentStatus.Paid;
        application.Status = PromiseApplicationStatus.Paid;
        application.ReviewedByOfficerId = officerId;
        application.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        await _auditService.LogAsync("WpaVerifyPromisePayment", "PromiseApplication", applicationId.ToString(),
            oldValues: new { PaymentStatus = oldPaymentStatus, Status = oldStatus },
            newValues: new { PaymentStatus = application.PaymentStatus, Status = application.Status });
    }

    private WpaPromiseApplicationDto MapToWpaPromiseApplicationDto(PromiseApplication pa) => new()
    {
        Id = pa.Id,
        CitizenId = pa.CitizenId,
        CitizenName = $"{_encryptionService.Decrypt(pa.Citizen.FirstNameEncrypted)} {_encryptionService.Decrypt(pa.Citizen.LastNameEncrypted)}",
        CitizenPesel = _encryptionService.Decrypt(pa.Citizen.PeselEncrypted),
        PermitId = pa.PermitId,
        PermitNumber = pa.Permit.PermitNumber,
        PermitType = pa.Permit.PermitType.ToString(),
        RequestedWeaponType = pa.RequestedWeaponType,
        RequestedQuantity = pa.RequestedQuantity,
        Status = pa.Status,
        RejectionReason = pa.RejectionReason,
        CorrectionNotes = pa.CorrectionNotes,
        CreatedAt = pa.CreatedAt,
        ReviewedAt = pa.ReviewedAt,
        ReviewedByOfficerName = pa.ReviewedByOfficer?.Email,
        FeeAmount = pa.FeeAmount,
        PaymentStatus = pa.PaymentStatus,
        Attachments = pa.Attachments.Select(a => new WpaPromiseApplicationAttachmentDto
        {
            Id = a.Id,
            AttachmentType = a.AttachmentType.ToString(),
            AttachmentTypeName = a.AttachmentType.ToString(),
            FileName = a.FileName,
            ContentType = a.ContentType,
            FileSize = a.FileSize,
            CreatedAt = a.CreatedAt
        }).ToList()
    };

    private WpaPermitApplicationDto MapToWpaPermitApplicationDto(PermitApplication pa) => new()
    {
        Id = pa.Id,
        CitizenId = pa.CitizenId,
        CitizenName = $"{_encryptionService.Decrypt(pa.Citizen.FirstNameEncrypted)} {_encryptionService.Decrypt(pa.Citizen.LastNameEncrypted)}",
        CitizenPesel = _encryptionService.Decrypt(pa.Citizen.PeselEncrypted),
        RequestedPermitType = pa.RequestedPermitType,
        RequestedPermitTypeName = pa.RequestedPermitType.ToString(),
        Reason = pa.Reason,
        MedicalExamExpiryDate = _encryptionService.DecryptDate(pa.MedicalExamExpiryDateEncrypted),
        PsychologicalExamExpiryDate = _encryptionService.DecryptDate(pa.PsychologicalExamExpiryDateEncrypted),
        Status = pa.Status,
        RejectionReason = pa.RejectionReason,
        CorrectionNotes = pa.CorrectionNotes,
        CreatedAt = pa.CreatedAt,
        ReviewedAt = pa.ReviewedAt,
        ReviewedByOfficerName = pa.ReviewedByOfficer?.Email,
        FeeAmount = pa.FeeAmount,
        PaymentStatus = pa.PaymentStatus,
        Attachments = pa.Attachments.Select(a => new WpaPermitApplicationAttachmentDto
        {
            Id = a.Id,
            AttachmentType = a.AttachmentType.ToString(),
            AttachmentTypeName = a.AttachmentType.ToString(),
            FileName = a.FileName,
            ContentType = a.ContentType,
            FileSize = a.FileSize,
            CreatedAt = a.CreatedAt
        }).ToList()
    };
}
