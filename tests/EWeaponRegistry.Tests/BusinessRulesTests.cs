using EWeaponRegistry.Application.DTOs.Citizen;
using EWeaponRegistry.Application.DTOs.Shop;
using EWeaponRegistry.Application.Exceptions;
using EWeaponRegistry.Application.Interfaces;
using EWeaponRegistry.Domain.Entities;
using EWeaponRegistry.Domain.Enums;
using EWeaponRegistry.Infrastructure.Data;
using EWeaponRegistry.Infrastructure.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Moq;

namespace EWeaponRegistry.Tests;

public class BusinessRulesTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly Mock<IEncryptionService> _encryptionMock;
    private readonly Mock<IAuditService> _auditMock;

    public BusinessRulesTests()
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
    }

    public void Dispose() => _context.Dispose();

    private CitizenProfile CreateCitizen(Guid? userId = null)
    {
        var citizen = new CitizenProfile
        {
            Id = Guid.NewGuid(),
            UserId = userId ?? Guid.NewGuid(),
            PeselEncrypted = "12345678901",
            FirstNameEncrypted = "Jan",
            LastNameEncrypted = "Kowalski",
            AddressEncrypted = "ul. Testowa 1",
            DocumentNumberEncrypted = "ABC123456",
            WeaponBookNumberEncrypted = "WB-001",
            CreatedAt = DateTime.UtcNow
        };
        _context.CitizenProfiles.Add(citizen);
        return citizen;
    }

    private Permit CreatePermit(
        Guid citizenId,
        bool active = true,
        bool expired = false,
        int maxFirearms = 5,
        int usedSlots = 0,
        PermitType permitType = PermitType.Sport)
    {
        var permit = new Permit
        {
            Id = Guid.NewGuid(),
            CitizenId = citizenId,
            PermitNumber = $"PRM-{Guid.NewGuid():N}"[..16],
            PermitType = permitType,
            Status = active ? PermitStatus.Active : PermitStatus.Suspended,
            IssueDate = DateTime.UtcNow.AddYears(-1),
            ExpiryDate = expired ? DateTime.UtcNow.AddDays(-1) : DateTime.UtcNow.AddYears(2),
            MaxFirearms = maxFirearms,
            UsedSlots = usedSlots,
            MedicalExamExpiryDateEncrypted = DateTime.UtcNow.AddYears(1).ToString("O"),
            PsychologicalExamExpiryDateEncrypted = DateTime.UtcNow.AddYears(1).ToString("O"),
            CreatedAt = DateTime.UtcNow
        };
        _context.Permits.Add(permit);
        return permit;
    }

    private Promise CreatePromise(
        Guid citizenId,
        Guid permitId,
        bool active = true,
        bool expired = false,
        int quantity = 2,
        int usedQuantity = 0,
        string? qrToken = null)
    {
        var promise = new Promise
        {
            Id = Guid.NewGuid(),
            CitizenId = citizenId,
            PermitId = permitId,
            PromiseNumber = $"PRO-{Guid.NewGuid():N}"[..16],
            WeaponType = "Pistolet",
            Quantity = quantity,
            UsedQuantity = usedQuantity,
            Status = active ? PromiseStatus.Active : PromiseStatus.Expired,
            FeeAmount = 150,
            PaymentStatus = PaymentStatus.Paid,
            QrToken = qrToken ?? Guid.NewGuid().ToString(),
            IssueDate = DateTime.UtcNow.AddDays(-10),
            ExpiryDate = expired ? DateTime.UtcNow.AddDays(-1) : DateTime.UtcNow.AddDays(30),
            CreatedAt = DateTime.UtcNow
        };
        _context.Promises.Add(promise);
        return promise;
    }

    private Shop CreateShop(Guid? userId = null, bool verified = true)
    {
        var shop = new Shop
        {
            Id = Guid.NewGuid(),
            UserId = userId ?? Guid.NewGuid(),
            Name = "Test Gun Shop",
            LicenseNumber = "LIC-001",
            IsVerified = verified,
            CreatedAt = DateTime.UtcNow
        };
        _context.Shops.Add(shop);
        return shop;
    }

    // Test 1: Próba rejestracji broni z już istniejącym numerem seryjnym
    [Fact]
    public async Task RegisterSale_DuplicateSerialNumber_ThrowsConflictException()
    {
        // Arrange
        var shopUserId = Guid.NewGuid();
        var citizenUserId = Guid.NewGuid();

        var shop = CreateShop(shopUserId);
        var citizen = CreateCitizen(citizenUserId);
        var permit = CreatePermit(citizen.Id);
        var promise = CreatePromise(citizen.Id, permit.Id);

        var existingFirearm = new Firearm
        {
            Id = Guid.NewGuid(),
            OwnerCitizenId = citizen.Id,
            Brand = "Glock",
            Model = "17",
            Category = FirearmCategory.B,
            Caliber = "9mm",
            SerialNumber = "SN-DUPLICATE-001",
            ProductionYear = 2020,
            Status = FirearmStatus.Registered,
            RegisteredAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        _context.Firearms.Add(existingFirearm);
        await _context.SaveChangesAsync();

        var service = new ShopService(_context, _encryptionMock.Object, _auditMock.Object);

        var request = new RegisterSaleRequest
        {
            QrToken = promise.QrToken,
            SerialNumber = "SN-DUPLICATE-001",
            Brand = "Walther",
            Model = "P99",
            Category = FirearmCategory.B,
            Caliber = "9mm",
            ProductionYear = 2021
        };

        // Act & Assert
        await Assert.ThrowsAsync<ConflictException>(() =>
            service.RegisterSaleAsync(shopUserId, request));
    }

    // Test 2: Próba sprzedaży bez ważnej promesy
    [Fact]
    public async Task RegisterSale_PromiseNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var shopUserId = Guid.NewGuid();
        CreateShop(shopUserId);
        await _context.SaveChangesAsync();

        var service = new ShopService(_context, _encryptionMock.Object, _auditMock.Object);

        var request = new RegisterSaleRequest
        {
            QrToken = "NIEISTNIEJACY-TOKEN-QR",
            SerialNumber = "SN-001",
            Brand = "Glock",
            Model = "17",
            Category = FirearmCategory.B,
            Caliber = "9mm",
            ProductionYear = 2020
        };

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            service.RegisterSaleAsync(shopUserId, request));
    }

    // Test 3: Próba złożenia wniosku o promesę gdy pozwolenie jest pełne
    [Fact]
    public async Task CreatePromiseApplication_PermitSlotsFull_ThrowsBusinessRuleViolation()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var citizen = CreateCitizen(userId);
        var permit = CreatePermit(citizen.Id, maxFirearms: 2, usedSlots: 2);
        await _context.SaveChangesAsync();

        var service = new CitizenService(_context, _encryptionMock.Object, _auditMock.Object);

        var request = new CreatePromiseApplicationRequest
        {
            PermitId = permit.Id,
            RequestedWeaponType = "Pistolet",
            RequestedQuantity = 1
        };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<BusinessRuleViolationException>(() =>
            service.CreatePromiseApplicationAsync(userId, request));

        ex.Message.Should().Contain("slots");
    }

    // Test 4: Próba złożenia wniosku na wygasłe pozwolenie
    [Fact]
    public async Task CreatePromiseApplication_ExpiredPermit_ThrowsBusinessRuleViolation()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var citizen = CreateCitizen(userId);
        var permit = CreatePermit(citizen.Id, expired: true);
        await _context.SaveChangesAsync();

        var service = new CitizenService(_context, _encryptionMock.Object, _auditMock.Object);

        var request = new CreatePromiseApplicationRequest
        {
            PermitId = permit.Id,
            RequestedWeaponType = "Pistolet",
            RequestedQuantity = 1
        };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<BusinessRuleViolationException>(() =>
            service.CreatePromiseApplicationAsync(userId, request));

        ex.Message.Should().Contain("expired");
    }

    // Test 5: Prawidłowy transfer broni między obywatelami
    [Fact]
    public async Task AcceptTransfer_ValidRequest_TransfersFirearmOwnership()
    {
        // Arrange
        var sellerUserId = Guid.NewGuid();
        var buyerUserId = Guid.NewGuid();

        var seller = CreateCitizen(sellerUserId);
        var buyer = CreateCitizen(buyerUserId);
        // Kupujący musi mieć ważne pozwolenie sportowe (akceptuje kategorię A)
        var buyerPermit = CreatePermit(buyer.Id, permitType: PermitType.Sport);

        var firearm = new Firearm
        {
            Id = Guid.NewGuid(),
            OwnerCitizenId = seller.Id,
            Brand = "Glock",
            Model = "17",
            Category = FirearmCategory.A,
            Caliber = "9mm",
            SerialNumber = "SN-TRANSFER-001",
            ProductionYear = 2020,
            Status = FirearmStatus.Registered,
            RegisteredAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        _context.Firearms.Add(firearm);

        var transferRequest = new TransferRequest
        {
            Id = Guid.NewGuid(),
            FirearmId = firearm.Id,
            SellerCitizenId = seller.Id,
            BuyerCitizenId = buyer.Id,
            BuyerPeselEncrypted = buyer.PeselEncrypted,
            TransferType = TransferType.Sale,
            Status = TransferRequestStatus.PendingAcceptance,
            CreatedAt = DateTime.UtcNow
        };
        _context.TransferRequests.Add(transferRequest);
        await _context.SaveChangesAsync();

        var service = new CitizenService(_context, _encryptionMock.Object, _auditMock.Object);

        // Act
        await service.AcceptTransferRequestAsync(buyerUserId, transferRequest.Id);

        // Assert - broń zmieniła właściciela
        var updatedFirearm = await _context.Firearms.FindAsync(firearm.Id);
        updatedFirearm!.OwnerCitizenId.Should().Be(buyer.Id);

        // Transfer oznaczony jako ukończony
        var updatedTransfer = await _context.TransferRequests.FindAsync(transferRequest.Id);
        updatedTransfer!.Status.Should().Be(TransferRequestStatus.Completed);

        // Historia własności zapisana
        var history = await _context.OwnershipHistories
            .FirstOrDefaultAsync(h => h.FirearmId == firearm.Id);
        history.Should().NotBeNull();
        history!.NewOwnerCitizenId.Should().Be(buyer.Id);
        history.PreviousOwnerCitizenId.Should().Be(seller.Id);
        history.TransferType.Should().Be(TransferType.Sale);
    }

    // Test 6: Audit log rejestruje krytyczne operacje
    [Fact]
    public async Task AuditLog_LogAsync_CreatesEntryInDatabase()
    {
        // Arrange
        var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        mockHttpContextAccessor.Setup(x => x.HttpContext).Returns((HttpContext?)null);
        var mockLogger = new Mock<ILogger<AuditService>>();

        var auditService = new AuditService(_context, mockHttpContextAccessor.Object, mockLogger.Object);

        // Act
        await auditService.LogAsync(
            action: "TestAction",
            entityType: "TestEntity",
            entityId: "test-id-123",
            newValues: new { Field = "TestValue" },
            description: "Testowy wpis audit logu");

        // Assert
        var logs = await _context.AuditLogs.ToListAsync();
        logs.Should().HaveCount(1);

        var log = logs[0];
        log.Action.Should().Be("TestAction");
        log.EntityType.Should().Be("TestEntity");
        log.EntityId.Should().Be("test-id-123");
        log.NewValuesJson.Should().Contain("TestValue");
        log.Description.Should().Be("Testowy wpis audit logu");
        log.TimestampUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        log.UserId.Should().BeNull(); // brak HTTP context = brak user ID
    }
}
