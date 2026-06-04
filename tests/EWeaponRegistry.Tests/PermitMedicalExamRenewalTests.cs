using EWeaponRegistry.Application.DTOs.Citizen;
using EWeaponRegistry.Application.DTOs.Wpa;
using EWeaponRegistry.Application.Exceptions;
using EWeaponRegistry.Application.Interfaces;
using EWeaponRegistry.Domain.Entities;
using EWeaponRegistry.Domain.Enums;
using EWeaponRegistry.Infrastructure.Data;
using EWeaponRegistry.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;

namespace EWeaponRegistry.Tests;

public class PermitMedicalExamRenewalTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly Mock<IEncryptionService> _encryptionMock;
    private readonly Mock<IAuditService> _auditMock;
    private readonly PermitMedicalExamRenewalService _service;

    public PermitMedicalExamRenewalTests()
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
        _service = new PermitMedicalExamRenewalService(_context, _encryptionMock.Object, _auditMock.Object);
    }

    public void Dispose() => _context.Dispose();

    private (User user, CitizenProfile citizen, Permit permit) SeedCitizenWithPermit()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "c@test.com",
            PasswordHash = "x",
            Role = UserRole.Citizen,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        var citizen = new CitizenProfile
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            PeselEncrypted = "1",
            FirstNameEncrypted = "Jan",
            LastNameEncrypted = "Kowalski",
            AddressEncrypted = "a",
            DocumentNumberEncrypted = "d",
            WeaponBookNumberEncrypted = "w",
            CreatedAt = DateTime.UtcNow
        };
        var permit = new Permit
        {
            Id = Guid.NewGuid(),
            CitizenId = citizen.Id,
            PermitNumber = "PZ-TEST",
            PermitType = PermitType.Sport,
            Status = PermitStatus.Active,
            IssueDate = DateTime.UtcNow.AddYears(-1),
            ExpiryDate = DateTime.UtcNow.AddYears(2),
            MaxFirearms = 5,
            UsedSlots = 0,
            MedicalExamExpiryDateEncrypted = DateTime.UtcNow.AddDays(-1).ToString("O"),
            PsychologicalExamExpiryDateEncrypted = DateTime.UtcNow.AddYears(1).ToString("O"),
            CreatedAt = DateTime.UtcNow
        };
        _context.Users.Add(user);
        _context.CitizenProfiles.Add(citizen);
        _context.Permits.Add(permit);
        _context.SaveChanges();
        return (user, citizen, permit);
    }

    private static RenewalCertificateUpload Cert(string name = "cert.pdf") =>
        new(name, "application/pdf", [0x25, 0x50, 0x44, 0x46]);

    [Fact]
    public async Task SubmitRenewal_WhenPendingExists_ThrowsConflict()
    {
        var (user, _, permit) = SeedCitizenWithPermit();
        var request = new SubmitPermitMedicalExamRenewalRequest
        {
            MedicalExamExpiryDate = DateTime.UtcNow.AddYears(1),
            PsychologicalExamExpiryDate = DateTime.UtcNow.AddYears(1)
        };

        await _service.SubmitRenewalAsync(user.Id, permit.Id, request, Cert("m.pdf"), Cert("p.pdf"));

        var act = () => _service.SubmitRenewalAsync(user.Id, permit.Id, request, Cert(), Cert());
        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task ApproveRenewal_UpdatesPermitDates()
    {
        var (user, _, permit) = SeedCitizenWithPermit();
        var medical = DateTime.UtcNow.AddYears(1).Date;
        var psych = DateTime.UtcNow.AddYears(2).Date;

        var renewal = await _service.SubmitRenewalAsync(
            user.Id,
            permit.Id,
            new SubmitPermitMedicalExamRenewalRequest { MedicalExamExpiryDate = medical, PsychologicalExamExpiryDate = psych },
            Cert("m.pdf"),
            Cert("p.pdf"));

        await _service.ApproveRenewalAsync(Guid.NewGuid(), renewal.Id, new ApprovePermitMedicalExamRenewalRequest());

        var updated = await _context.Permits.FindAsync(permit.Id);
        _encryptionMock.Object.DecryptDate(updated!.MedicalExamExpiryDateEncrypted)!.Value.Date.Should().Be(medical);
        _encryptionMock.Object.DecryptDate(updated.PsychologicalExamExpiryDateEncrypted)!.Value.Date.Should().Be(psych);
    }

    [Fact]
    public async Task RejectRenewal_LeavesPermitDatesUnchanged()
    {
        var (user, _, permit) = SeedCitizenWithPermit();
        var originalMedical = _encryptionMock.Object.DecryptDate(permit.MedicalExamExpiryDateEncrypted);

        var renewal = await _service.SubmitRenewalAsync(
            user.Id,
            permit.Id,
            new SubmitPermitMedicalExamRenewalRequest
            {
                MedicalExamExpiryDate = DateTime.UtcNow.AddYears(1),
                PsychologicalExamExpiryDate = DateTime.UtcNow.AddYears(1)
            },
            Cert(),
            Cert());

        await _service.RejectRenewalAsync(Guid.NewGuid(), renewal.Id, new RejectPermitMedicalExamRenewalRequest { Reason = "Nieczytelne skan" });

        var updated = await _context.Permits.FindAsync(permit.Id);
        _encryptionMock.Object.DecryptDate(updated!.MedicalExamExpiryDateEncrypted).Should().Be(originalMedical);
    }

    [Fact]
    public async Task ApproveRenewal_RetainsPriorRenewalHistory()
    {
        var (user, _, permit) = SeedCitizenWithPermit();
        var first = await _service.SubmitRenewalAsync(
            user.Id,
            permit.Id,
            new SubmitPermitMedicalExamRenewalRequest
            {
                MedicalExamExpiryDate = DateTime.UtcNow.AddYears(1),
                PsychologicalExamExpiryDate = DateTime.UtcNow.AddYears(1)
            },
            Cert("first-m.pdf"),
            Cert("first-p.pdf"));

        await _service.ApproveRenewalAsync(Guid.NewGuid(), first.Id, new ApprovePermitMedicalExamRenewalRequest());

        var second = await _service.SubmitRenewalAsync(
            user.Id,
            permit.Id,
            new SubmitPermitMedicalExamRenewalRequest
            {
                MedicalExamExpiryDate = DateTime.UtcNow.AddYears(2),
                PsychologicalExamExpiryDate = DateTime.UtcNow.AddYears(2)
            },
            Cert("second-m.pdf"),
            Cert("second-p.pdf"));

        await _service.ApproveRenewalAsync(Guid.NewGuid(), second.Id, new ApprovePermitMedicalExamRenewalRequest());

        var all = await _context.PermitMedicalExamRenewals.Where(r => r.PermitId == permit.Id).ToListAsync();
        all.Should().HaveCount(2);
        all.Should().Contain(r => r.Id == first.Id && r.Status == PermitMedicalExamRenewalStatus.Approved);
        all.Should().Contain(r => r.Id == second.Id && r.Status == PermitMedicalExamRenewalStatus.Approved);

        var firstAttachments = await _context.PermitMedicalExamRenewalAttachments
            .Where(a => a.PermitMedicalExamRenewalId == first.Id)
            .ToListAsync();
        firstAttachments.Should().HaveCount(2);
        firstAttachments.Should().Contain(a => a.FileName == "first-m.pdf");
    }
}
