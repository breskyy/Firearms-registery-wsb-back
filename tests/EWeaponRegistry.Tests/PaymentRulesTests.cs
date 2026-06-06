using EWeaponRegistry.Application.DTOs.Citizen;
using EWeaponRegistry.Application.DTOs.Wpa;
using EWeaponRegistry.Application.Exceptions;
using EWeaponRegistry.Application.Interfaces;
using EWeaponRegistry.Application.Interfaces.ExternalGateways;
using EWeaponRegistry.Domain.Constants;
using EWeaponRegistry.Domain.Entities;
using EWeaponRegistry.Domain.Enums;
using EWeaponRegistry.Infrastructure.Data;
using EWeaponRegistry.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;

namespace EWeaponRegistry.Tests;

public class PaymentRulesTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly Mock<IEncryptionService> _encryptionMock;
    private readonly Mock<IAuditService> _auditMock;
    private readonly Mock<IPaymentGateway> _paymentGatewayMock;
    private readonly Mock<IMObywatelGateway> _mObywatelGatewayMock;

    public PaymentRulesTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        _context = new AppDbContext(options);

        _encryptionMock = new Mock<IEncryptionService>();
        _encryptionMock.Setup(e => e.Encrypt(It.IsAny<string>())).Returns<string>(s => s);
        _encryptionMock.Setup(e => e.Decrypt(It.IsAny<string>())).Returns<string>(s => s);
        _encryptionMock.Setup(e => e.EncryptDate(It.IsAny<DateTime>()))
            .Returns<DateTime>(d => d.ToString("O"));
        _encryptionMock.Setup(e => e.DecryptDate(It.IsAny<string>()))
            .Returns<string>(s => string.IsNullOrEmpty(s) ? null : DateTime.Parse(s));

        _auditMock = new Mock<IAuditService>();
        _paymentGatewayMock = new Mock<IPaymentGateway>();
        _mObywatelGatewayMock = new Mock<IMObywatelGateway>();
        _mObywatelGatewayMock
            .Setup(g => g.GenerateQrTokenAsync(It.IsAny<string>(), It.IsAny<Guid>()))
            .ReturnsAsync(new MObywatelQrResult { Success = true, QrToken = "QR-TEST" });
    }

    public void Dispose() => _context.Dispose();

    private CitizenService CreateCitizenService() =>
        new(_context, _encryptionMock.Object, _auditMock.Object, _paymentGatewayMock.Object);

    private WpaService CreateWpaService() =>
        new(_context, _encryptionMock.Object, _auditMock.Object, _mObywatelGatewayMock.Object);

    private (CitizenProfile citizen, Guid userId) SeedCitizen()
    {
        var userId = Guid.NewGuid();
        var citizen = new CitizenProfile
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            PeselEncrypted = "12345678901",
            FirstNameEncrypted = "Jan",
            LastNameEncrypted = "Kowalski",
            AddressEncrypted = "ul. Testowa 1",
            DocumentNumberEncrypted = "ABC123456",
            WeaponBookNumberEncrypted = "WB-001",
            CreatedAt = DateTime.UtcNow
        };
        _context.CitizenProfiles.Add(citizen);
        return (citizen, userId);
    }

    [Fact]
    public async Task CreatePermitApplication_SetsPendingPaymentWith242PlnFee()
    {
        var (citizen, userId) = SeedCitizen();
        await _context.SaveChangesAsync();
        var service = CreateCitizenService();

        var result = await service.CreatePermitApplicationAsync(userId, new CreatePermitApplicationRequest
        {
            RequestedPermitType = PermitType.Sport,
            Reason = "Sport strzelecki — wniosek testowy opis"
        });

        result.FeeAmount.Should().Be(ApplicationPaymentFees.PermitApplicationFee);
        result.PaymentStatus.Should().Be(PaymentStatus.Pending);

        var entity = await _context.PermitApplications.FindAsync(result.Id);
        entity!.PaymentStatus.Should().Be(PaymentStatus.Pending);
    }

    [Fact]
    public async Task CreatePromiseApplication_SetsPendingPaymentWithCalculatedFee()
    {
        var (citizen, userId) = SeedCitizen();
        var permit = new Permit
        {
            Id = Guid.NewGuid(),
            CitizenId = citizen.Id,
            PermitNumber = "PRM-001",
            PermitType = PermitType.Sport,
            Status = PermitStatus.Active,
            IssueDate = DateTime.UtcNow.AddYears(-1),
            ExpiryDate = DateTime.UtcNow.AddYears(2),
            MaxFirearms = 5,
            UsedSlots = 0,
            CreatedAt = DateTime.UtcNow
        };
        _context.Permits.Add(permit);
        await _context.SaveChangesAsync();
        var service = CreateCitizenService();

        var result = await service.CreatePromiseApplicationAsync(userId, new CreatePromiseApplicationRequest
        {
            PermitId = permit.Id,
            RequestedWeaponType = "Pistolet Glock",
            RequestedQuantity = 2
        });

        result.FeeAmount.Should().Be(34m);
        result.PaymentStatus.Should().Be(PaymentStatus.Pending);
    }

    [Fact]
    public async Task ConfirmPermitPayment_SetsSubmittedStatus()
    {
        var (citizen, userId) = SeedCitizen();
        var application = new PermitApplication
        {
            Id = Guid.NewGuid(),
            CitizenId = citizen.Id,
            RequestedPermitType = PermitType.Sport,
            Reason = "Test",
            Status = PermitApplicationStatus.Submitted,
            FeeAmount = ApplicationPaymentFees.PermitApplicationFee,
            PaymentStatus = PaymentStatus.Pending,
            PaymentReferenceId = "PAY-TEST-001",
            CreatedAt = DateTime.UtcNow
        };
        _context.PermitApplications.Add(application);
        await _context.SaveChangesAsync();

        _paymentGatewayMock
            .Setup(g => g.ConfirmPaymentAsync("PAY-TEST-001"))
            .ReturnsAsync(new PaymentConfirmResult { Success = true, IsPaid = true, TransactionId = "TXN-1" });

        var service = CreateCitizenService();
        var result = await service.ConfirmPermitApplicationPaymentAsync(userId, application.Id, "PAY-TEST-001");

        result.PaymentStatus.Should().Be(PaymentStatus.Submitted);
    }

    [Fact]
    public async Task MarkPermitUnderReview_WithoutVerifiedPayment_ThrowsBusinessRuleViolation()
    {
        var officerId = Guid.NewGuid();
        var (citizen, _) = SeedCitizen();
        var application = new PermitApplication
        {
            Id = Guid.NewGuid(),
            CitizenId = citizen.Id,
            RequestedPermitType = PermitType.Sport,
            Reason = "Test",
            Status = PermitApplicationStatus.Submitted,
            FeeAmount = ApplicationPaymentFees.PermitApplicationFee,
            PaymentStatus = PaymentStatus.Submitted,
            PaymentMethod = PaymentMethod.OnlineMock,
            PaymentReferenceId = "PAY-ONLINE-1",
            CreatedAt = DateTime.UtcNow
        };
        _context.PermitApplications.Add(application);
        await _context.SaveChangesAsync();

        var wpaService = CreateWpaService();
        var ex = await Assert.ThrowsAsync<BusinessRuleViolationException>(() =>
            wpaService.MarkPermitApplicationUnderReviewAsync(officerId, application.Id));

        ex.Message.Should().Contain("payment");
    }

    [Fact]
    public async Task VerifyPermitPayment_AllowsUnderReview()
    {
        var officerId = Guid.NewGuid();
        var (citizen, _) = SeedCitizen();
        var application = new PermitApplication
        {
            Id = Guid.NewGuid(),
            CitizenId = citizen.Id,
            RequestedPermitType = PermitType.Sport,
            Reason = "Test",
            Status = PermitApplicationStatus.Submitted,
            FeeAmount = ApplicationPaymentFees.PermitApplicationFee,
            PaymentStatus = PaymentStatus.Submitted,
            PaymentMethod = PaymentMethod.OnlineMock,
            PaymentReferenceId = "PAY-ONLINE-1",
            CreatedAt = DateTime.UtcNow
        };
        _context.PermitApplications.Add(application);
        await _context.SaveChangesAsync();

        var wpaService = CreateWpaService();
        await wpaService.VerifyPermitApplicationPaymentAsync(officerId, application.Id);
        await wpaService.MarkPermitApplicationUnderReviewAsync(officerId, application.Id);

        var updated = await _context.PermitApplications.FindAsync(application.Id);
        updated!.PaymentStatus.Should().Be(PaymentStatus.Paid);
        updated.Status.Should().Be(PermitApplicationStatus.UnderReview);
    }

    [Fact]
    public async Task VerifyPromisePayment_SetsApplicationStatusPaid()
    {
        var officerId = Guid.NewGuid();
        var (citizen, _) = SeedCitizen();
        var permit = new Permit
        {
            Id = Guid.NewGuid(),
            CitizenId = citizen.Id,
            PermitNumber = "PRM-002",
            PermitType = PermitType.Sport,
            Status = PermitStatus.Active,
            IssueDate = DateTime.UtcNow.AddYears(-1),
            ExpiryDate = DateTime.UtcNow.AddYears(2),
            MaxFirearms = 5,
            UsedSlots = 0,
            CreatedAt = DateTime.UtcNow
        };
        var application = new PromiseApplication
        {
            Id = Guid.NewGuid(),
            CitizenId = citizen.Id,
            PermitId = permit.Id,
            RequestedWeaponType = "Pistolet",
            RequestedQuantity = 1,
            Status = PromiseApplicationStatus.Submitted,
            FeeAmount = 17m,
            PaymentStatus = PaymentStatus.Submitted,
            PaymentMethod = PaymentMethod.OnlineMock,
            PaymentReferenceId = "PAY-PROMISE-1",
            CreatedAt = DateTime.UtcNow
        };
        _context.Permits.Add(permit);
        _context.PromiseApplications.Add(application);
        await _context.SaveChangesAsync();

        var wpaService = CreateWpaService();
        await wpaService.VerifyPromiseApplicationPaymentAsync(officerId, application.Id);

        var updated = await _context.PromiseApplications.FindAsync(application.Id);
        updated!.PaymentStatus.Should().Be(PaymentStatus.Paid);
        updated.Status.Should().Be(PromiseApplicationStatus.Paid);
    }

    [Fact]
    public async Task ApprovePromiseApplication_WithoutVerifiedPayment_ThrowsBusinessRuleViolation()
    {
        var officerId = Guid.NewGuid();
        var (citizen, _) = SeedCitizen();
        var permit = new Permit
        {
            Id = Guid.NewGuid(),
            CitizenId = citizen.Id,
            PermitNumber = "PRM-003",
            PermitType = PermitType.Sport,
            Status = PermitStatus.Active,
            IssueDate = DateTime.UtcNow.AddYears(-1),
            ExpiryDate = DateTime.UtcNow.AddYears(2),
            MaxFirearms = 5,
            UsedSlots = 0,
            CreatedAt = DateTime.UtcNow
        };
        var application = new PromiseApplication
        {
            Id = Guid.NewGuid(),
            CitizenId = citizen.Id,
            PermitId = permit.Id,
            RequestedWeaponType = "Pistolet",
            RequestedQuantity = 1,
            Status = PromiseApplicationStatus.UnderReview,
            FeeAmount = 17m,
            PaymentStatus = PaymentStatus.Submitted,
            CreatedAt = DateTime.UtcNow
        };
        _context.Permits.Add(permit);
        _context.PromiseApplications.Add(application);
        await _context.SaveChangesAsync();

        var wpaService = CreateWpaService();
        var ex = await Assert.ThrowsAsync<BusinessRuleViolationException>(() =>
            wpaService.ApproveApplicationAsync(officerId, application.Id));

        ex.Message.Should().Contain("payment");
    }

    [Fact]
    public async Task UploadPermitPaymentProof_SetsBankTransferMethodAndSubmittedStatus()
    {
        var (citizen, userId) = SeedCitizen();
        var application = new PermitApplication
        {
            Id = Guid.NewGuid(),
            CitizenId = citizen.Id,
            RequestedPermitType = PermitType.Sport,
            Reason = "Test",
            Status = PermitApplicationStatus.Submitted,
            FeeAmount = ApplicationPaymentFees.PermitApplicationFee,
            PaymentStatus = PaymentStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };
        _context.PermitApplications.Add(application);
        await _context.SaveChangesAsync();

        var service = CreateCitizenService();
        await service.UploadPermitApplicationPaymentProofAsync(
            userId,
            application.Id,
            "proof.pdf",
            "application/pdf",
            new byte[] { 1, 2, 3 });

        var updated = await _context.PermitApplications
            .Include(a => a.Attachments)
            .FirstAsync(a => a.Id == application.Id);

        updated.PaymentStatus.Should().Be(PaymentStatus.Submitted);
        updated.PaymentMethod.Should().Be(PaymentMethod.BankTransfer);
        updated.Attachments.Should().ContainSingle(a => a.AttachmentType == PermitApplicationAttachmentType.PaymentProof);
    }

    [Fact]
    public async Task UploadPermitPaymentProof_WithInvalidMime_ThrowsBusinessRuleViolation()
    {
        var (citizen, userId) = SeedCitizen();
        var application = new PermitApplication
        {
            Id = Guid.NewGuid(),
            CitizenId = citizen.Id,
            RequestedPermitType = PermitType.Sport,
            Reason = "Test",
            Status = PermitApplicationStatus.Submitted,
            FeeAmount = ApplicationPaymentFees.PermitApplicationFee,
            PaymentStatus = PaymentStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };
        _context.PermitApplications.Add(application);
        await _context.SaveChangesAsync();

        var service = CreateCitizenService();
        var ex = await Assert.ThrowsAsync<BusinessRuleViolationException>(() =>
            service.UploadPermitApplicationPaymentProofAsync(
                userId,
                application.Id,
                "proof.exe",
                "application/octet-stream",
                new byte[] { 1 }));

        ex.Message.Should().Contain("PDF");
    }

    [Fact]
    public async Task VerifyPermitPayment_WithoutProofForBankTransfer_ThrowsBusinessRuleViolation()
    {
        var officerId = Guid.NewGuid();
        var (citizen, _) = SeedCitizen();
        var application = new PermitApplication
        {
            Id = Guid.NewGuid(),
            CitizenId = citizen.Id,
            RequestedPermitType = PermitType.Sport,
            Reason = "Test",
            Status = PermitApplicationStatus.Submitted,
            FeeAmount = ApplicationPaymentFees.PermitApplicationFee,
            PaymentStatus = PaymentStatus.Submitted,
            PaymentMethod = PaymentMethod.BankTransfer,
            CreatedAt = DateTime.UtcNow
        };
        _context.PermitApplications.Add(application);
        await _context.SaveChangesAsync();

        var wpaService = CreateWpaService();
        var ex = await Assert.ThrowsAsync<BusinessRuleViolationException>(() =>
            wpaService.VerifyPermitApplicationPaymentAsync(officerId, application.Id));

        ex.Message.Should().Contain("proof");
    }

    [Fact]
    public async Task RejectPermitPaymentProof_ReturnsToPendingWithComment()
    {
        var officerId = Guid.NewGuid();
        var (citizen, _) = SeedCitizen();
        var application = new PermitApplication
        {
            Id = Guid.NewGuid(),
            CitizenId = citizen.Id,
            RequestedPermitType = PermitType.Sport,
            Reason = "Test",
            Status = PermitApplicationStatus.Submitted,
            FeeAmount = ApplicationPaymentFees.PermitApplicationFee,
            PaymentStatus = PaymentStatus.Submitted,
            PaymentMethod = PaymentMethod.BankTransfer,
            CreatedAt = DateTime.UtcNow,
            Attachments =
            {
                new PermitApplicationAttachment
                {
                    Id = Guid.NewGuid(),
                    AttachmentType = PermitApplicationAttachmentType.PaymentProof,
                    FileName = "proof.pdf",
                    ContentType = "application/pdf",
                    FileSize = 3,
                    Content = new byte[] { 1, 2, 3 },
                    CreatedAt = DateTime.UtcNow
                }
            }
        };
        _context.PermitApplications.Add(application);
        await _context.SaveChangesAsync();

        var wpaService = CreateWpaService();
        await wpaService.RejectPermitApplicationPaymentProofAsync(officerId, application.Id, "Kwota wpłaty nie zgadza się z wymaganą opłatą skarbową.");

        var updated = await _context.PermitApplications
            .Include(a => a.Attachments)
            .FirstAsync(a => a.Id == application.Id);

        updated.PaymentStatus.Should().Be(PaymentStatus.Pending);
        updated.PaymentRejectionComment.Should().Contain("Kwota wpłaty");
        updated.PaymentMethod.Should().BeNull();
        updated.Attachments.Should().BeEmpty();
    }

    [Fact]
    public async Task ConfirmPermitPayment_SetsOnlineMockMethod()
    {
        var (citizen, userId) = SeedCitizen();
        var application = new PermitApplication
        {
            Id = Guid.NewGuid(),
            CitizenId = citizen.Id,
            RequestedPermitType = PermitType.Sport,
            Reason = "Test",
            Status = PermitApplicationStatus.Submitted,
            FeeAmount = ApplicationPaymentFees.PermitApplicationFee,
            PaymentStatus = PaymentStatus.Pending,
            PaymentReferenceId = "PAY-TEST-002",
            PaymentMethod = PaymentMethod.OnlineMock,
            CreatedAt = DateTime.UtcNow
        };
        _context.PermitApplications.Add(application);
        await _context.SaveChangesAsync();

        _paymentGatewayMock
            .Setup(g => g.ConfirmPaymentAsync("PAY-TEST-002"))
            .ReturnsAsync(new PaymentConfirmResult { Success = true, IsPaid = true, TransactionId = "TXN-2" });

        var service = CreateCitizenService();
        var result = await service.ConfirmPermitApplicationPaymentAsync(userId, application.Id, "PAY-TEST-002");

        result.PaymentStatus.Should().Be(PaymentStatus.Submitted);
        result.PaymentMethod.Should().Be(PaymentMethod.OnlineMock);
    }
}
