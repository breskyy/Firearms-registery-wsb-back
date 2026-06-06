using EWeaponRegistry.Application.DTOs.Citizen;
using EWeaponRegistry.Application.Exceptions;
using EWeaponRegistry.Application.Interfaces;
using EWeaponRegistry.Application.Interfaces.ExternalGateways;
using EWeaponRegistry.Domain.Constants;
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
    private readonly IPaymentGateway _paymentGateway;

    public CitizenService(
        AppDbContext context,
        IEncryptionService encryptionService,
        IAuditService auditService,
        IPaymentGateway paymentGateway)
    {
        _context = context;
        _encryptionService = encryptionService;
        _auditService = auditService;
        _paymentGateway = paymentGateway;
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
            .Include(c => c.PromiseApplications)
            .ThenInclude(pa => pa.Attachments)
            .FirstOrDefaultAsync(c => c.UserId == userId)
            ?? throw new NotFoundException("Citizen profile not found");

        return citizen.PromiseApplications.Select(MapPromiseApplicationDto).ToList();
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
            FeeAmount = ApplicationPaymentFees.CalculatePromiseFee(request.RequestedQuantity),
            PaymentStatus = PaymentStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        _context.PromiseApplications.Add(application);
        await _context.SaveChangesAsync();

        await _auditService.LogAsync("CreatePromiseApplication", "PromiseApplication", application.Id.ToString(),
            newValues: new { request.PermitId, request.RequestedWeaponType, request.RequestedQuantity });

        return MapPromiseApplicationDto(application, permit.PermitNumber);
    }

    public async Task<PromiseApplicationDto> UpdatePromiseApplicationCorrectionAsync(
        Guid userId,
        Guid applicationId,
        UpdatePromiseApplicationCorrectionRequest request)
    {
        var citizen = await _context.CitizenProfiles
            .Include(c => c.Permits)
            .FirstOrDefaultAsync(c => c.UserId == userId)
            ?? throw new NotFoundException("Citizen profile not found");

        var application = await _context.PromiseApplications
            .Include(pa => pa.Permit)
            .FirstOrDefaultAsync(pa => pa.Id == applicationId && pa.CitizenId == citizen.Id)
            ?? throw new NotFoundException("Promise application", applicationId);

        if (application.Status != PromiseApplicationStatus.RequiresCorrection)
            throw new BusinessRuleViolationException(
                $"Cannot submit correction for application (current status: {application.Status})");

        var permit = citizen.Permits.FirstOrDefault(p => p.Id == request.PermitId)
            ?? throw new NotFoundException("Permit", request.PermitId);

        if (permit.Status != PermitStatus.Active)
            throw new BusinessRuleViolationException("Permit is not active");

        if (permit.ExpiryDate < DateTime.UtcNow.Date)
            throw new BusinessRuleViolationException("Permit has expired");

        if (permit.UsedSlots + request.RequestedQuantity > permit.MaxFirearms)
            throw new BusinessRuleViolationException(
                $"Requested quantity ({request.RequestedQuantity}) exceeds available slots ({permit.MaxFirearms - permit.UsedSlots})");

        application.PermitId = permit.Id;
        application.RequestedWeaponType = request.RequestedWeaponType;
        application.RequestedQuantity = request.RequestedQuantity;
        application.Status = PromiseApplicationStatus.Submitted;
        application.FeeAmount = ApplicationPaymentFees.CalculatePromiseFee(request.RequestedQuantity);
        application.PaymentStatus = PaymentStatus.Pending;
        application.PaymentReferenceId = null;
        application.PaymentMethod = null;
        application.PaymentRejectionComment = null;
        application.CorrectionNotes = null;
        application.ReviewedByOfficerId = null;
        application.ReviewedAt = null;
        application.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        await _auditService.LogAsync("CorrectPromiseApplication", "PromiseApplication", application.Id.ToString(),
            newValues: new { request.PermitId, request.RequestedWeaponType, request.RequestedQuantity });

        return MapPromiseApplicationDto(application, permit.PermitNumber);
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

            // Find buyer by PESEL (with permits eagerly loaded for eligibility check)
            var buyerPeselEncrypted = _encryptionService.Encrypt(request.BuyerPesel);
            var allCitizens = await _context.CitizenProfiles
                .Include(c => c.Permits)
                .ToListAsync();
            var buyer = allCitizens.FirstOrDefault(c =>
                _encryptionService.Decrypt(c.PeselEncrypted) == request.BuyerPesel);

            if (buyer == null)
                throw new NotFoundException("Buyer with provided PESEL not found in the system");

            if (buyer.Id == seller.Id)
                throw new BusinessRuleViolationException("Cannot transfer firearm to yourself");

            // Pre-validate buyer eligibility (RODO-safe: errors only reveal pass/fail, not buyer's permit details).
            // This prevents orphan PendingAcceptance transfers that the buyer would never be able to accept.
            var matchingPermits = buyer.Permits
                .Where(p => IsPermitValidForCategory(p.PermitType, firearm.Category))
                .ToList();

            if (matchingPermits.Count == 0)
                throw new BusinessRuleViolationException("Buyer does not have a permit covering this firearm category");

            var activePermit = matchingPermits.FirstOrDefault(p =>
                p.Status == PermitStatus.Active && p.ExpiryDate >= DateTime.UtcNow.Date);

            if (activePermit == null)
                throw new BusinessRuleViolationException("Buyer's matching permit is not active or has expired");

            if (activePermit.UsedSlots >= activePermit.MaxFirearms)
                throw new BusinessRuleViolationException("Buyer has no free slots on their matching permit");

            var buyerMedicalExpiry = _encryptionService.DecryptDate(activePermit.MedicalExamExpiryDateEncrypted);
            var buyerPsychExpiry = _encryptionService.DecryptDate(activePermit.PsychologicalExamExpiryDateEncrypted);
            if (buyerMedicalExpiry < DateTime.UtcNow.Date || buyerPsychExpiry < DateTime.UtcNow.Date)
                throw new BusinessRuleViolationException("Buyer's medical or psychological exam has expired");

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

    public async Task CancelTransferRequestAsync(Guid userId, Guid transferRequestId)
    {
        var seller = await _context.CitizenProfiles
            .FirstOrDefaultAsync(c => c.UserId == userId)
            ?? throw new NotFoundException("Citizen profile not found");

        var transferRequest = await _context.TransferRequests
            .FirstOrDefaultAsync(tr => tr.Id == transferRequestId)
            ?? throw new NotFoundException("Transfer request", transferRequestId);

        if (transferRequest.SellerCitizenId != seller.Id)
            throw new ForbiddenException("Only the seller can cancel a transfer request");

        if (transferRequest.Status != TransferRequestStatus.PendingAcceptance)
            throw new BusinessRuleViolationException("Only pending transfer requests can be cancelled");

        transferRequest.Status = TransferRequestStatus.Cancelled;
        transferRequest.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        await _auditService.LogAsync("CancelTransfer", "TransferRequest", transferRequestId.ToString());
    }

    public async Task<IList<PermitApplicationDto>> GetMyPermitApplicationsAsync(Guid userId)
    {
        var citizen = await _context.CitizenProfiles
            .Include(c => c.PermitApplications)
            .ThenInclude(pa => pa.Attachments)
            .FirstOrDefaultAsync(c => c.UserId == userId)
            ?? throw new NotFoundException("Citizen profile not found");

        return citizen.PermitApplications.Select(MapPermitApplicationDto).ToList();
    }

    public async Task<PermitApplicationDto> CreatePermitApplicationAsync(Guid userId, CreatePermitApplicationRequest request)
    {
        var citizen = await _context.CitizenProfiles
            .FirstOrDefaultAsync(c => c.UserId == userId)
            ?? throw new NotFoundException("Citizen profile not found");

        var application = new PermitApplication
        {
            Id = Guid.NewGuid(),
            CitizenId = citizen.Id,
            RequestedPermitType = request.RequestedPermitType,
            Reason = request.Reason,
            MedicalExamExpiryDateEncrypted = request.MedicalExamExpiryDate.HasValue
                ? _encryptionService.EncryptDate(request.MedicalExamExpiryDate.Value)
                : null,
            PsychologicalExamExpiryDateEncrypted = request.PsychologicalExamExpiryDate.HasValue
                ? _encryptionService.EncryptDate(request.PsychologicalExamExpiryDate.Value)
                : null,
            Status = PermitApplicationStatus.Submitted,
            FeeAmount = ApplicationPaymentFees.PermitApplicationFee,
            PaymentStatus = PaymentStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        _context.PermitApplications.Add(application);
        await _context.SaveChangesAsync();

        await _auditService.LogAsync("CreatePermitApplication", "PermitApplication", application.Id.ToString(),
            newValues: new { request.RequestedPermitType, request.Reason });

        return MapPermitApplicationDto(application);
    }

    public async Task<PermitApplicationDto> UpdatePermitApplicationCorrectionAsync(
        Guid userId,
        Guid applicationId,
        UpdatePermitApplicationCorrectionRequest request)
    {
        var citizen = await _context.CitizenProfiles
            .FirstOrDefaultAsync(c => c.UserId == userId)
            ?? throw new NotFoundException("Citizen profile not found");

        var application = await _context.PermitApplications
            .FirstOrDefaultAsync(pa => pa.Id == applicationId && pa.CitizenId == citizen.Id)
            ?? throw new NotFoundException("Permit application", applicationId);

        if (application.Status != PermitApplicationStatus.RequiresCorrection)
            throw new BusinessRuleViolationException(
                $"Cannot submit correction for application (current status: {application.Status})");

        application.RequestedPermitType = request.RequestedPermitType;
        application.Reason = request.Reason;
        application.MedicalExamExpiryDateEncrypted = request.MedicalExamExpiryDate.HasValue
            ? _encryptionService.EncryptDate(request.MedicalExamExpiryDate.Value)
            : null;
        application.PsychologicalExamExpiryDateEncrypted = request.PsychologicalExamExpiryDate.HasValue
            ? _encryptionService.EncryptDate(request.PsychologicalExamExpiryDate.Value)
            : null;
        application.Status = PermitApplicationStatus.Submitted;
        application.PaymentStatus = PaymentStatus.Pending;
        application.PaymentReferenceId = null;
        application.PaymentMethod = null;
        application.PaymentRejectionComment = null;
        application.CorrectionNotes = null;
        application.ReviewedByOfficerId = null;
        application.ReviewedAt = null;
        application.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        await _auditService.LogAsync("CorrectPermitApplication", "PermitApplication", application.Id.ToString(),
            newValues: new { request.RequestedPermitType, request.Reason });

        return MapPermitApplicationDto(application);
    }

    public async Task<ApplicationPaymentDto> InitiatePermitApplicationPaymentAsync(Guid userId, Guid applicationId)
    {
        var application = await GetOwnedPermitApplicationAsync(userId, applicationId, includeAttachments: false);
        EnsurePaymentCanBeInitiated(application.PaymentStatus);

        var result = await _paymentGateway.InitiatePaymentAsync(
            application.FeeAmount,
            $"Opłata skarbowa — wniosek o pozwolenie {application.Id}",
            application.Id.ToString());

        if (!result.Success || string.IsNullOrEmpty(result.PaymentId))
            throw new BusinessRuleViolationException(result.ErrorMessage ?? "Payment initiation failed");

        application.PaymentReferenceId = result.PaymentId;
        application.PaymentMethod = PaymentMethod.OnlineMock;
        application.PaymentRejectionComment = null;
        application.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        await _auditService.LogAsync("InitiatePermitPayment", "PermitApplication", applicationId.ToString(),
            newValues: new { result.PaymentId, application.FeeAmount });

        return MapApplicationPaymentDto(application, result.PaymentUrl);
    }

    public async Task<ApplicationPaymentDto> ConfirmPermitApplicationPaymentAsync(Guid userId, Guid applicationId, string paymentId)
    {
        var application = await GetOwnedPermitApplicationAsync(userId, applicationId, includeAttachments: false);
        await ConfirmApplicationPaymentAsync(application, paymentId, "PermitApplication", "ConfirmPermitPayment");
        return MapApplicationPaymentDto(application);
    }

    public async Task<PermitApplicationAttachmentDto> UploadPermitApplicationPaymentProofAsync(
        Guid userId,
        Guid applicationId,
        string fileName,
        string contentType,
        byte[] content)
    {
        var application = await GetOwnedPermitApplicationAsync(userId, applicationId, includeAttachments: true);
        EnsurePaymentProofCanBeSubmitted(application.PaymentStatus);
        ValidatePaymentProofContentType(contentType);
        if (content.Length > 10 * 1024 * 1024)
            throw new BusinessRuleViolationException("Payment proof file cannot exceed 10 MB");

        var attachment = await ReplacePermitAttachmentAsync(
            application,
            PermitApplicationAttachmentType.PaymentProof,
            fileName,
            contentType,
            content);

        application.PaymentStatus = PaymentStatus.Submitted;
        application.PaymentMethod = PaymentMethod.BankTransfer;
        application.PaymentReferenceId = null;
        application.PaymentRejectionComment = null;
        application.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        await _auditService.LogAsync("SubmitPermitPaymentProof", "PermitApplication", applicationId.ToString(),
            newValues: new { attachment.FileName, application.FeeAmount });

        return MapPermitAttachmentDto(attachment);
    }

    public async Task<ApplicationPaymentDto> InitiatePromiseApplicationPaymentAsync(Guid userId, Guid applicationId)
    {
        var application = await GetOwnedPromiseApplicationAsync(userId, applicationId, includeAttachments: false);
        EnsurePaymentCanBeInitiated(application.PaymentStatus);

        var result = await _paymentGateway.InitiatePaymentAsync(
            application.FeeAmount,
            $"Opłata skarbowa — wniosek o promesę {application.Id}",
            application.Id.ToString());

        if (!result.Success || string.IsNullOrEmpty(result.PaymentId))
            throw new BusinessRuleViolationException(result.ErrorMessage ?? "Payment initiation failed");

        application.PaymentReferenceId = result.PaymentId;
        application.PaymentMethod = PaymentMethod.OnlineMock;
        application.PaymentRejectionComment = null;
        application.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        await _auditService.LogAsync("InitiatePromisePayment", "PromiseApplication", applicationId.ToString(),
            newValues: new { result.PaymentId, application.FeeAmount });

        return MapApplicationPaymentDto(application, result.PaymentUrl);
    }

    public async Task<ApplicationPaymentDto> ConfirmPromiseApplicationPaymentAsync(Guid userId, Guid applicationId, string paymentId)
    {
        var application = await GetOwnedPromiseApplicationAsync(userId, applicationId, includeAttachments: false);
        await ConfirmApplicationPaymentAsync(application, paymentId, "PromiseApplication", "ConfirmPromisePayment");
        return MapApplicationPaymentDto(application);
    }

    public async Task<PromiseApplicationAttachmentDto> UploadPromiseApplicationPaymentProofAsync(
        Guid userId,
        Guid applicationId,
        string fileName,
        string contentType,
        byte[] content)
    {
        var application = await GetOwnedPromiseApplicationAsync(userId, applicationId, includeAttachments: true);
        EnsurePaymentProofCanBeSubmitted(application.PaymentStatus);
        ValidatePaymentProofContentType(contentType);
        if (content.Length > 10 * 1024 * 1024)
            throw new BusinessRuleViolationException("Payment proof file cannot exceed 10 MB");

        var attachment = await ReplacePromiseAttachmentAsync(
            application,
            PromiseApplicationAttachmentType.PaymentProof,
            fileName,
            contentType,
            content);

        application.PaymentStatus = PaymentStatus.Submitted;
        application.PaymentMethod = PaymentMethod.BankTransfer;
        application.PaymentReferenceId = null;
        application.PaymentRejectionComment = null;
        application.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        await _auditService.LogAsync("SubmitPromisePaymentProof", "PromiseApplication", applicationId.ToString(),
            newValues: new { attachment.FileName, application.FeeAmount });

        return MapPromiseAttachmentDto(attachment);
    }

    public async Task<IList<CitizenMedicalAlertDto>> GetMyMedicalAlertsAsync(Guid userId)
    {
        var citizen = await _context.CitizenProfiles
            .Include(c => c.MedicalAlerts)
            .ThenInclude(ma => ma.Permit)
            .FirstOrDefaultAsync(c => c.UserId == userId)
            ?? throw new NotFoundException("Citizen profile not found");

        return citizen.MedicalAlerts.Select(ma => new CitizenMedicalAlertDto
        {
            Id = ma.Id,
            PermitId = ma.PermitId,
            PermitNumber = ma.Permit?.PermitNumber,
            AlertType = ma.AlertType,
            Message = ma.Message,
            DueDate = ma.DueDate,
            IsResolved = ma.IsResolved,
            CreatedAt = ma.CreatedAt
        }).ToList();
    }

    public async Task ReportFirearmLostAsync(Guid userId, Guid firearmId, ReportFirearmLostRequest request)
    {
        var citizen = await _context.CitizenProfiles
            .FirstOrDefaultAsync(c => c.UserId == userId)
            ?? throw new NotFoundException("Citizen profile not found");

        var firearm = await _context.Firearms
            .FirstOrDefaultAsync(f => f.Id == firearmId && f.OwnerCitizenId == citizen.Id)
            ?? throw new NotFoundException("Firearm", firearmId);

        if (firearm.Status != FirearmStatus.Registered)
            throw new BusinessRuleViolationException("Only registered firearms can be reported as lost");

        firearm.Status = FirearmStatus.Lost;
        firearm.UpdatedAt = DateTime.UtcNow;

        // Free up the permit slot
        var permit = await _context.Permits
            .Where(p => p.CitizenId == citizen.Id && p.Status == PermitStatus.Active)
            .FirstOrDefaultAsync();
        if (permit != null)
            permit.UsedSlots = Math.Max(0, permit.UsedSlots - 1);

        await _context.SaveChangesAsync();

        await _auditService.LogAsync("ReportFirearmLost", "Firearm", firearmId.ToString(),
            oldValues: new { Status = FirearmStatus.Registered },
            newValues: new { Status = FirearmStatus.Lost, request.Description });
    }

    private async Task<PermitApplication> GetOwnedPermitApplicationAsync(Guid userId, Guid applicationId, bool includeAttachments)
    {
        var citizen = await _context.CitizenProfiles
            .FirstOrDefaultAsync(c => c.UserId == userId)
            ?? throw new NotFoundException("Citizen profile not found");

        IQueryable<PermitApplication> query = _context.PermitApplications;
        if (includeAttachments)
            query = query.Include(pa => pa.Attachments);

        return await query.FirstOrDefaultAsync(pa => pa.Id == applicationId && pa.CitizenId == citizen.Id)
            ?? throw new NotFoundException("Permit application", applicationId);
    }

    private async Task<PromiseApplication> GetOwnedPromiseApplicationAsync(Guid userId, Guid applicationId, bool includeAttachments)
    {
        var citizen = await _context.CitizenProfiles
            .FirstOrDefaultAsync(c => c.UserId == userId)
            ?? throw new NotFoundException("Citizen profile not found");

        IQueryable<PromiseApplication> query = _context.PromiseApplications;
        if (includeAttachments)
            query = query.Include(pa => pa.Attachments);

        return await query.FirstOrDefaultAsync(pa => pa.Id == applicationId && pa.CitizenId == citizen.Id)
            ?? throw new NotFoundException("Promise application", applicationId);
    }

    private static void EnsurePaymentCanBeInitiated(PaymentStatus paymentStatus)
    {
        if (paymentStatus is PaymentStatus.Paid or PaymentStatus.Submitted)
            throw new BusinessRuleViolationException($"Payment cannot be initiated (current status: {paymentStatus})");
    }

    private static void EnsurePaymentProofCanBeSubmitted(PaymentStatus paymentStatus)
    {
        if (paymentStatus is PaymentStatus.Paid or PaymentStatus.Submitted)
            throw new BusinessRuleViolationException($"Payment proof cannot be submitted (current status: {paymentStatus})");
    }

    private static void ValidatePaymentProofContentType(string contentType)
    {
        var allowed = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "application/pdf",
            "image/jpeg",
            "image/png"
        };

        if (!allowed.Contains(contentType))
            throw new BusinessRuleViolationException("Only PDF, JPG and PNG files are allowed for payment proof");
    }

    private async Task ConfirmApplicationPaymentAsync(
        PermitApplication application,
        string paymentId,
        string entityType,
        string auditAction)
    {
        if (!string.Equals(application.PaymentReferenceId, paymentId, StringComparison.OrdinalIgnoreCase))
            throw new BusinessRuleViolationException("Payment reference does not match this application");

        if (application.PaymentStatus is PaymentStatus.Paid or PaymentStatus.Submitted)
            throw new BusinessRuleViolationException($"Payment already processed (current status: {application.PaymentStatus})");

        var result = await _paymentGateway.ConfirmPaymentAsync(paymentId);
        if (!result.Success || !result.IsPaid)
            throw new BusinessRuleViolationException(result.ErrorMessage ?? "Payment confirmation failed");

        application.PaymentStatus = PaymentStatus.Submitted;
        application.PaymentMethod = PaymentMethod.OnlineMock;
        application.PaymentRejectionComment = null;
        application.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        await _auditService.LogAsync(auditAction, entityType, application.Id.ToString(),
            newValues: new { paymentId, result.TransactionId, application.FeeAmount });
    }

    private async Task ConfirmApplicationPaymentAsync(
        PromiseApplication application,
        string paymentId,
        string entityType,
        string auditAction)
    {
        if (!string.Equals(application.PaymentReferenceId, paymentId, StringComparison.OrdinalIgnoreCase))
            throw new BusinessRuleViolationException("Payment reference does not match this application");

        if (application.PaymentStatus is PaymentStatus.Paid or PaymentStatus.Submitted)
            throw new BusinessRuleViolationException($"Payment already processed (current status: {application.PaymentStatus})");

        var result = await _paymentGateway.ConfirmPaymentAsync(paymentId);
        if (!result.Success || !result.IsPaid)
            throw new BusinessRuleViolationException(result.ErrorMessage ?? "Payment confirmation failed");

        application.PaymentStatus = PaymentStatus.Submitted;
        application.PaymentMethod = PaymentMethod.OnlineMock;
        application.PaymentRejectionComment = null;
        application.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        await _auditService.LogAsync(auditAction, entityType, application.Id.ToString(),
            newValues: new { paymentId, result.TransactionId, application.FeeAmount });
    }

    private async Task<PermitApplicationAttachment> ReplacePermitAttachmentAsync(
        PermitApplication application,
        PermitApplicationAttachmentType type,
        string fileName,
        string contentType,
        byte[] content)
    {
        var existing = application.Attachments.FirstOrDefault(a => a.AttachmentType == type);
        if (existing != null)
        {
            application.Attachments.Remove(existing);
            _context.PermitApplicationAttachments.Remove(existing);
        }

        var attachment = new PermitApplicationAttachment
        {
            Id = Guid.NewGuid(),
            PermitApplicationId = application.Id,
            AttachmentType = type,
            FileName = fileName,
            ContentType = contentType,
            FileSize = content.Length,
            Content = content,
            CreatedAt = DateTime.UtcNow
        };

        await _context.PermitApplicationAttachments.AddAsync(attachment);
        return attachment;
    }

    private async Task<PromiseApplicationAttachment> ReplacePromiseAttachmentAsync(
        PromiseApplication application,
        PromiseApplicationAttachmentType type,
        string fileName,
        string contentType,
        byte[] content)
    {
        var existing = application.Attachments.FirstOrDefault(a => a.AttachmentType == type);
        if (existing != null)
        {
            application.Attachments.Remove(existing);
            _context.PromiseApplicationAttachments.Remove(existing);
        }

        var attachment = new PromiseApplicationAttachment
        {
            Id = Guid.NewGuid(),
            PromiseApplicationId = application.Id,
            AttachmentType = type,
            FileName = fileName,
            ContentType = contentType,
            FileSize = content.Length,
            Content = content,
            CreatedAt = DateTime.UtcNow
        };

        await _context.PromiseApplicationAttachments.AddAsync(attachment);
        return attachment;
    }

    private PermitApplicationDto MapPermitApplicationDto(PermitApplication pa) => new()
    {
        Id = pa.Id,
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
        FeeAmount = pa.FeeAmount,
        PaymentStatus = pa.PaymentStatus,
        PaymentMethod = pa.PaymentMethod,
        PaymentRejectionComment = pa.PaymentRejectionComment,
        Attachments = pa.Attachments.Select(MapPermitAttachmentDto).ToList()
    };

    private static PermitApplicationAttachmentDto MapPermitAttachmentDto(PermitApplicationAttachment a) => new()
    {
        Id = a.Id,
        AttachmentType = a.AttachmentType.ToString(),
        AttachmentTypeName = a.AttachmentType.ToString(),
        FileName = a.FileName,
        ContentType = a.ContentType,
        FileSize = a.FileSize,
        CreatedAt = a.CreatedAt
    };

    private PromiseApplicationDto MapPromiseApplicationDto(PromiseApplication pa) =>
        MapPromiseApplicationDto(pa, pa.Permit?.PermitNumber ?? string.Empty);

    private static PromiseApplicationDto MapPromiseApplicationDto(PromiseApplication pa, string permitNumber) => new()
    {
        Id = pa.Id,
        PermitId = pa.PermitId,
        PermitNumber = permitNumber,
        RequestedWeaponType = pa.RequestedWeaponType,
        RequestedQuantity = pa.RequestedQuantity,
        Status = pa.Status,
        RejectionReason = pa.RejectionReason,
        CorrectionNotes = pa.CorrectionNotes,
        CreatedAt = pa.CreatedAt,
        ReviewedAt = pa.ReviewedAt,
        FeeAmount = pa.FeeAmount,
        PaymentStatus = pa.PaymentStatus,
        PaymentMethod = pa.PaymentMethod,
        PaymentRejectionComment = pa.PaymentRejectionComment,
        Attachments = pa.Attachments.Select(MapPromiseAttachmentDto).ToList()
    };

    private static PromiseApplicationAttachmentDto MapPromiseAttachmentDto(PromiseApplicationAttachment a) => new()
    {
        Id = a.Id,
        AttachmentType = a.AttachmentType.ToString(),
        AttachmentTypeName = a.AttachmentType.ToString(),
        FileName = a.FileName,
        ContentType = a.ContentType,
        FileSize = a.FileSize,
        CreatedAt = a.CreatedAt
    };

    private static ApplicationPaymentDto MapApplicationPaymentDto(PermitApplication application, string? paymentUrl = null) => new()
    {
        ApplicationId = application.Id,
        FeeAmount = application.FeeAmount,
        PaymentStatus = application.PaymentStatus,
        PaymentMethod = application.PaymentMethod,
        PaymentReferenceId = application.PaymentReferenceId,
        PaymentUrl = paymentUrl,
        PaymentRejectionComment = application.PaymentRejectionComment,
        BankTransferDetails = BuildBankTransferDetails(application.Id, application.FeeAmount, "permit")
    };

    private static ApplicationPaymentDto MapApplicationPaymentDto(PromiseApplication application, string? paymentUrl = null) => new()
    {
        ApplicationId = application.Id,
        FeeAmount = application.FeeAmount,
        PaymentStatus = application.PaymentStatus,
        PaymentMethod = application.PaymentMethod,
        PaymentReferenceId = application.PaymentReferenceId,
        PaymentUrl = paymentUrl,
        PaymentRejectionComment = application.PaymentRejectionComment,
        BankTransferDetails = BuildBankTransferDetails(application.Id, application.FeeAmount, "promise")
    };

    private static BankTransferDetailsDto BuildBankTransferDetails(Guid applicationId, decimal amount, string kind) => new()
    {
        AccountNumber = ApplicationPaymentBankDetails.AccountNumber,
        AccountHolder = ApplicationPaymentBankDetails.AccountHolder,
        BankName = ApplicationPaymentBankDetails.BankName,
        TransferTitle = ApplicationPaymentBankDetails.BuildTransferTitle(applicationId, kind),
        Amount = amount
    };

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
