using EWeaponRegistry.Application.DTOs.Citizen;
using EWeaponRegistry.Application.DTOs.Common;
using EWeaponRegistry.Application.DTOs.Wpa;
using EWeaponRegistry.Application.Exceptions;
using EWeaponRegistry.Application.Interfaces;
using EWeaponRegistry.Domain.Entities;
using EWeaponRegistry.Domain.Enums;
using EWeaponRegistry.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EWeaponRegistry.Infrastructure.Services;

public class PermitMedicalExamRenewalService : IPermitMedicalExamRenewalService
{
    private const long MaxFileSize = 10 * 1024 * 1024;
    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "application/pdf",
        "image/jpeg",
        "image/png"
    };

    private readonly AppDbContext _context;
    private readonly IEncryptionService _encryptionService;
    private readonly IAuditService _auditService;

    public PermitMedicalExamRenewalService(
        AppDbContext context,
        IEncryptionService encryptionService,
        IAuditService auditService)
    {
        _context = context;
        _encryptionService = encryptionService;
        _auditService = auditService;
    }

    public async Task<PermitMedicalExamRenewalDto> SubmitRenewalAsync(
        Guid userId,
        Guid permitId,
        SubmitPermitMedicalExamRenewalRequest request,
        RenewalCertificateUpload medicalCertificate,
        RenewalCertificateUpload psychologicalCertificate)
    {
        var citizen = await GetCitizenByUserIdAsync(userId);
        var permit = await GetOwnedActivePermitAsync(citizen.Id, permitId);

        await EnsureNoPendingRenewalAsync(permitId);
        ValidateProposedDates(request.MedicalExamExpiryDate, request.PsychologicalExamExpiryDate);
        ValidateCertificate(medicalCertificate, "medical");
        ValidateCertificate(psychologicalCertificate, "psychological");

        var renewal = new PermitMedicalExamRenewal
        {
            Id = Guid.NewGuid(),
            PermitId = permit.Id,
            CitizenId = citizen.Id,
            Status = PermitMedicalExamRenewalStatus.Submitted,
            ProposedMedicalExpiryDateEncrypted = _encryptionService.EncryptDate(request.MedicalExamExpiryDate.Date),
            ProposedPsychologicalExpiryDateEncrypted = _encryptionService.EncryptDate(request.PsychologicalExamExpiryDate.Date),
            CreatedAt = DateTime.UtcNow
        };

        renewal.Attachments.Add(CreateAttachment(renewal.Id, PermitApplicationAttachmentType.MedicalCertificate, medicalCertificate));
        renewal.Attachments.Add(CreateAttachment(renewal.Id, PermitApplicationAttachmentType.PsychologicalCertificate, psychologicalCertificate));

        await _context.PermitMedicalExamRenewals.AddAsync(renewal);
        await _context.SaveChangesAsync();

        await _auditService.LogAsync("CitizenSubmitMedicalExamRenewal", "PermitMedicalExamRenewal", renewal.Id.ToString(),
            newValues: new { permitId, renewal.Status });

        return await MapCitizenDtoAsync(renewal.Id);
    }

    public async Task<IList<PermitMedicalExamRenewalDto>> GetMyRenewalsForPermitAsync(Guid userId, Guid permitId)
    {
        var citizen = await GetCitizenByUserIdAsync(userId);
        _ = await GetOwnedPermitAsync(citizen.Id, permitId);

        var renewals = await _context.PermitMedicalExamRenewals
            .AsNoTracking()
            .Include(r => r.Attachments)
            .Include(r => r.Permit)
            .Where(r => r.PermitId == permitId && r.CitizenId == citizen.Id)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        return renewals.Select(r => MapCitizenDto(r)).ToList();
    }

    public async Task<IList<PermitMedicalExamRenewalDto>> GetMyRenewalsAsync(Guid userId)
    {
        var citizen = await GetCitizenByUserIdAsync(userId);

        var renewals = await _context.PermitMedicalExamRenewals
            .AsNoTracking()
            .Include(r => r.Attachments)
            .Include(r => r.Permit)
            .Where(r => r.CitizenId == citizen.Id)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        return renewals.Select(r => MapCitizenDto(r)).ToList();
    }

    public async Task<PaginatedResult<WpaPermitMedicalExamRenewalDto>> GetRenewalsForWpaAsync(
        PermitMedicalExamRenewalStatus? status,
        PaginationParams pagination)
    {
        var query = _context.PermitMedicalExamRenewals
            .AsNoTracking()
            .Include(r => r.Attachments)
            .Include(r => r.Permit)
            .Include(r => r.Citizen)
            .AsQueryable();

        if (status.HasValue)
            query = query.Where(r => r.Status == status.Value);
        else
            query = query.Where(r => r.Status == PermitMedicalExamRenewalStatus.Submitted
                || r.Status == PermitMedicalExamRenewalStatus.UnderReview);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToListAsync();

        return new PaginatedResult<WpaPermitMedicalExamRenewalDto>
        {
            Items = items.Select(MapWpaDto).ToList(),
            TotalCount = total,
            Page = pagination.Page,
            PageSize = pagination.PageSize
        };
    }

    public async Task<WpaPermitMedicalExamRenewalDto?> GetRenewalByIdForWpaAsync(Guid renewalId)
    {
        var renewal = await _context.PermitMedicalExamRenewals
            .AsNoTracking()
            .Include(r => r.Attachments)
            .Include(r => r.Permit)
            .Include(r => r.Citizen)
            .FirstOrDefaultAsync(r => r.Id == renewalId);

        return renewal == null ? null : MapWpaDto(renewal);
    }

    public async Task MarkRenewalUnderReviewAsync(Guid officerUserId, Guid renewalId)
    {
        var renewal = await GetRenewalForUpdateAsync(renewalId);
        if (renewal.Status != PermitMedicalExamRenewalStatus.Submitted)
            throw new BusinessRuleViolationException($"Cannot mark renewal in status {renewal.Status} as under review");

        renewal.Status = PermitMedicalExamRenewalStatus.UnderReview;
        renewal.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        await _auditService.LogAsync("WpaMarkMedicalExamRenewalUnderReview", "PermitMedicalExamRenewal", renewalId.ToString(),
            newValues: new { officerUserId });
    }

    public async Task ApproveRenewalAsync(
        Guid officerUserId,
        Guid renewalId,
        ApprovePermitMedicalExamRenewalRequest request)
    {
        var renewal = await GetRenewalForUpdateAsync(renewalId);
        if (renewal.Status is not (PermitMedicalExamRenewalStatus.Submitted or PermitMedicalExamRenewalStatus.UnderReview))
            throw new BusinessRuleViolationException($"Cannot approve renewal in status {renewal.Status}");

        EnsureRequiredAttachments(renewal);

        var medicalDate = request.MedicalExamExpiryDate?.Date
            ?? _encryptionService.DecryptDate(renewal.ProposedMedicalExpiryDateEncrypted)!.Value.Date;
        var psychDate = request.PsychologicalExamExpiryDate?.Date
            ?? _encryptionService.DecryptDate(renewal.ProposedPsychologicalExpiryDateEncrypted)!.Value.Date;

        var permit = await _context.Permits.FirstAsync(p => p.Id == renewal.PermitId);
        permit.MedicalExamExpiryDateEncrypted = _encryptionService.EncryptDate(medicalDate);
        permit.PsychologicalExamExpiryDateEncrypted = _encryptionService.EncryptDate(psychDate);
        permit.UpdatedAt = DateTime.UtcNow;

        renewal.Status = PermitMedicalExamRenewalStatus.Approved;
        renewal.ReviewedAt = DateTime.UtcNow;
        renewal.ReviewedByOfficerUserId = officerUserId;
        renewal.UpdatedAt = DateTime.UtcNow;

        await ResolveMedicalAlertsForPermitAsync(renewal.CitizenId, renewal.PermitId);
        await _context.SaveChangesAsync();

        await _auditService.LogAsync("WpaApproveMedicalExamRenewal", "PermitMedicalExamRenewal", renewalId.ToString(),
            newValues: new { officerUserId, medicalDate, psychDate });
    }

    public async Task RejectRenewalAsync(
        Guid officerUserId,
        Guid renewalId,
        RejectPermitMedicalExamRenewalRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Reason))
            throw new BusinessRuleViolationException("Rejection reason is required");

        var renewal = await GetRenewalForUpdateAsync(renewalId);
        if (renewal.Status is not (PermitMedicalExamRenewalStatus.Submitted or PermitMedicalExamRenewalStatus.UnderReview))
            throw new BusinessRuleViolationException($"Cannot reject renewal in status {renewal.Status}");

        renewal.Status = PermitMedicalExamRenewalStatus.Rejected;
        renewal.RejectionReason = request.Reason.Trim();
        renewal.ReviewedAt = DateTime.UtcNow;
        renewal.ReviewedByOfficerUserId = officerUserId;
        renewal.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        await _auditService.LogAsync("WpaRejectMedicalExamRenewal", "PermitMedicalExamRenewal", renewalId.ToString(),
            newValues: new { officerUserId, request.Reason });
    }

    public async Task<(byte[] Content, string ContentType, string FileName)?> GetRenewalAttachmentAsync(
        Guid renewalId,
        Guid attachmentId,
        bool requireOfficer)
    {
        var attachment = await _context.PermitMedicalExamRenewalAttachments
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == attachmentId && a.PermitMedicalExamRenewalId == renewalId);

        if (attachment == null)
            return null;

        if (!requireOfficer)
        {
            var renewal = await _context.PermitMedicalExamRenewals.AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == renewalId);
            if (renewal == null)
                return null;
        }

        return (attachment.Content, attachment.ContentType, attachment.FileName);
    }

    private async Task ResolveMedicalAlertsForPermitAsync(Guid citizenId, Guid permitId)
    {
        var alerts = await _context.MedicalAlerts
            .Where(a => a.CitizenId == citizenId && a.PermitId == permitId && !a.IsResolved)
            .ToListAsync();

        foreach (var alert in alerts)
        {
            alert.IsResolved = true;
            alert.UpdatedAt = DateTime.UtcNow;
        }
    }

    private async Task<PermitMedicalExamRenewalDto> MapCitizenDtoAsync(Guid renewalId)
    {
        var renewal = await _context.PermitMedicalExamRenewals
            .AsNoTracking()
            .Include(r => r.Attachments)
            .Include(r => r.Permit)
            .FirstAsync(r => r.Id == renewalId);

        return MapCitizenDto(renewal);
    }

    private PermitMedicalExamRenewalDto MapCitizenDto(PermitMedicalExamRenewal renewal) =>
        new()
        {
            Id = renewal.Id,
            PermitId = renewal.PermitId,
            PermitNumber = renewal.Permit.PermitNumber,
            Status = renewal.Status.ToString(),
            StatusName = renewal.Status.ToString(),
            ProposedMedicalExamExpiryDate = DecryptRequiredDate(renewal.ProposedMedicalExpiryDateEncrypted),
            ProposedPsychologicalExamExpiryDate = DecryptRequiredDate(renewal.ProposedPsychologicalExpiryDateEncrypted),
            RejectionReason = renewal.RejectionReason,
            ReviewedAt = renewal.ReviewedAt,
            CreatedAt = renewal.CreatedAt,
            Attachments = renewal.Attachments.OrderBy(a => a.AttachmentType).Select(a => new PermitMedicalExamRenewalAttachmentDto
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

    private WpaPermitMedicalExamRenewalDto MapWpaDto(PermitMedicalExamRenewal renewal) =>
        new()
        {
            Id = renewal.Id,
            PermitId = renewal.PermitId,
            PermitNumber = renewal.Permit.PermitNumber,
            PermitTypeName = renewal.Permit.PermitType.ToString(),
            CitizenId = renewal.CitizenId,
            CitizenName = $"{_encryptionService.Decrypt(renewal.Citizen.FirstNameEncrypted)} {_encryptionService.Decrypt(renewal.Citizen.LastNameEncrypted)}",
            CitizenPesel = _encryptionService.Decrypt(renewal.Citizen.PeselEncrypted),
            Status = renewal.Status.ToString(),
            StatusName = renewal.Status.ToString(),
            ProposedMedicalExamExpiryDate = DecryptRequiredDate(renewal.ProposedMedicalExpiryDateEncrypted),
            ProposedPsychologicalExamExpiryDate = DecryptRequiredDate(renewal.ProposedPsychologicalExpiryDateEncrypted),
            RejectionReason = renewal.RejectionReason,
            ReviewedAt = renewal.ReviewedAt,
            CreatedAt = renewal.CreatedAt,
            Attachments = renewal.Attachments.OrderBy(a => a.AttachmentType).Select(a => new WpaPermitMedicalExamRenewalAttachmentDto
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

    private DateTime DecryptRequiredDate(string encrypted) =>
        _encryptionService.DecryptDate(encrypted) ?? throw new InvalidOperationException("Missing encrypted date");

    private async Task<CitizenProfile> GetCitizenByUserIdAsync(Guid userId)
    {
        return await _context.CitizenProfiles.FirstOrDefaultAsync(c => c.UserId == userId)
            ?? throw new NotFoundException("Citizen profile not found");
    }

    private async Task<Permit> GetOwnedActivePermitAsync(Guid citizenId, Guid permitId)
    {
        var permit = await _context.Permits.FirstOrDefaultAsync(p => p.Id == permitId && p.CitizenId == citizenId)
            ?? throw new NotFoundException("Permit", permitId);

        if (permit.Status != PermitStatus.Active)
            throw new BusinessRuleViolationException("Renewals are only allowed for active permits");

        return permit;
    }

    private async Task<Permit> GetOwnedPermitAsync(Guid citizenId, Guid permitId)
    {
        return await _context.Permits.FirstOrDefaultAsync(p => p.Id == permitId && p.CitizenId == citizenId)
            ?? throw new NotFoundException("Permit", permitId);
    }

    private async Task EnsureNoPendingRenewalAsync(Guid permitId)
    {
        var hasPending = await _context.PermitMedicalExamRenewals.AnyAsync(r =>
            r.PermitId == permitId &&
            (r.Status == PermitMedicalExamRenewalStatus.Submitted || r.Status == PermitMedicalExamRenewalStatus.UnderReview));

        if (hasPending)
            throw new ConflictException("A medical exam renewal is already pending for this permit");
    }

    private static void ValidateProposedDates(DateTime medical, DateTime psychological)
    {
        var today = DateTime.UtcNow.Date;
        if (medical.Date < today)
            throw new BusinessRuleViolationException("Proposed medical exam expiry date must be today or later");
        if (psychological.Date < today)
            throw new BusinessRuleViolationException("Proposed psychological exam expiry date must be today or later");
    }

    private static void ValidateCertificate(RenewalCertificateUpload upload, string label)
    {
        if (upload.Content.Length == 0)
            throw new BusinessRuleViolationException($"Missing {label} certificate");
        if (upload.Content.Length > MaxFileSize)
            throw new BusinessRuleViolationException("Attachment file cannot exceed 10 MB");
        if (!AllowedContentTypes.Contains(upload.ContentType))
            throw new BusinessRuleViolationException("Only PDF, JPG and PNG files are allowed");
    }

    private static PermitMedicalExamRenewalAttachment CreateAttachment(
        Guid renewalId,
        PermitApplicationAttachmentType type,
        RenewalCertificateUpload upload) =>
        new()
        {
            Id = Guid.NewGuid(),
            PermitMedicalExamRenewalId = renewalId,
            AttachmentType = type,
            FileName = upload.FileName,
            ContentType = upload.ContentType,
            FileSize = upload.Content.Length,
            Content = upload.Content,
            CreatedAt = DateTime.UtcNow
        };

    private static void EnsureRequiredAttachments(PermitMedicalExamRenewal renewal)
    {
        var hasMedical = renewal.Attachments.Any(a => a.AttachmentType == PermitApplicationAttachmentType.MedicalCertificate);
        var hasPsych = renewal.Attachments.Any(a => a.AttachmentType == PermitApplicationAttachmentType.PsychologicalCertificate);
        if (!hasMedical || !hasPsych)
            throw new BusinessRuleViolationException("Both medical and psychological certificates are required");
    }

    private async Task<PermitMedicalExamRenewal> GetRenewalForUpdateAsync(Guid renewalId)
    {
        return await _context.PermitMedicalExamRenewals
            .Include(r => r.Attachments)
            .FirstOrDefaultAsync(r => r.Id == renewalId)
            ?? throw new NotFoundException("Permit medical exam renewal", renewalId);
    }
}
