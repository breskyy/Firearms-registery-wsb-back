using EWeaponRegistry.Application.Interfaces;
using EWeaponRegistry.Domain.Entities;
using EWeaponRegistry.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EWeaponRegistry.Infrastructure.Data;

public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var encryptionService = scope.ServiceProvider.GetRequiredService<IEncryptionService>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<AppDbContext>>();

        try
        {
            // Apply pending migrations
            await context.Database.MigrateAsync();

            // Seed only if no users exist
            if (await context.Users.AnyAsync())
            {
                logger.LogInformation("Database already seeded");
                return;
            }

            logger.LogInformation("Seeding database...");

            // Create users
            var adminUser = new User
            {
                Id = Guid.NewGuid(),
                Email = "admin@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                Role = UserRole.Admin,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var officerUser = new User
            {
                Id = Guid.NewGuid(),
                Email = "officer@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Officer123!"),
                Role = UserRole.WpaOfficer,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var citizenUser = new User
            {
                Id = Guid.NewGuid(),
                Email = "citizen@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Citizen123!"),
                Role = UserRole.Citizen,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var shopUser = new User
            {
                Id = Guid.NewGuid(),
                Email = "shop@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Shop123!"),
                Role = UserRole.Shop,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            context.Users.AddRange(adminUser, officerUser, citizenUser, shopUser);

            // Create citizen profile
            var citizenProfile = new CitizenProfile
            {
                Id = Guid.NewGuid(),
                UserId = citizenUser.Id,
                FirstNameEncrypted = encryptionService.Encrypt("Jan"),
                LastNameEncrypted = encryptionService.Encrypt("Kowalski"),
                PeselEncrypted = encryptionService.Encrypt("90010112345"),
                AddressEncrypted = encryptionService.Encrypt("ul. Testowa 1, 00-001 Warszawa"),
                DocumentNumberEncrypted = encryptionService.Encrypt("ABC123456"),
                WeaponBookNumberEncrypted = encryptionService.Encrypt("WB-2024-00001"),
                CreatedAt = DateTime.UtcNow
            };

            context.CitizenProfiles.Add(citizenProfile);

            // Create shop
            var shop = new Shop
            {
                Id = Guid.NewGuid(),
                UserId = shopUser.Id,
                Name = "Sklep Myśliwski \"Jeleń\"",
                LicenseNumber = "SH-2024-00001",
                Address = "ul. Łowiecka 10, 00-002 Warszawa",
                IsVerified = true,
                CreatedAt = DateTime.UtcNow
            };

            context.Shops.Add(shop);

            // Create permit for citizen
            var permit = new Permit
            {
                Id = Guid.NewGuid(),
                CitizenId = citizenProfile.Id,
                PermitNumber = "PZ-2024-00001",
                PermitType = PermitType.Sport,
                Status = PermitStatus.Active,
                IssueDate = DateTime.UtcNow.AddYears(-1),
                ExpiryDate = DateTime.UtcNow.AddYears(4),
                MaxFirearms = 5,
                UsedSlots = 1,
                MedicalExamExpiryDateEncrypted = encryptionService.EncryptDate(DateTime.UtcNow.AddYears(1)),
                PsychologicalExamExpiryDateEncrypted = encryptionService.EncryptDate(DateTime.UtcNow.AddYears(1)),
                CreatedAt = DateTime.UtcNow
            };

            context.Permits.Add(permit);

            // Create promise
            var promise = new Promise
            {
                Id = Guid.NewGuid(),
                CitizenId = citizenProfile.Id,
                PermitId = permit.Id,
                PromiseNumber = "PROM-2024-00001",
                WeaponType = "Pistolet sportowy 9mm",
                Quantity = 2,
                UsedQuantity = 0,
                Status = PromiseStatus.Active,
                FeeAmount = 17.00m,
                PaymentStatus = PaymentStatus.Paid,
                QrToken = "QR-TEST-TOKEN-12345678",
                IssueDate = DateTime.UtcNow.AddDays(-10),
                ExpiryDate = DateTime.UtcNow.AddMonths(3),
                CreatedAt = DateTime.UtcNow
            };

            context.Promises.Add(promise);

            // Create firearm
            var firearm = new Firearm
            {
                Id = Guid.NewGuid(),
                OwnerCitizenId = citizenProfile.Id,
                Brand = "Glock",
                Model = "17 Gen5",
                Category = FirearmCategory.B,
                Caliber = "9x19mm Parabellum",
                SerialNumber = "GLOCK-2024-00001",
                ProductionYear = 2024,
                Status = FirearmStatus.Registered,
                RegisteredAt = DateTime.UtcNow.AddMonths(-6),
                CreatedAt = DateTime.UtcNow
            };

            context.Firearms.Add(firearm);

            // Create ownership history
            var ownershipHistory = new OwnershipHistory
            {
                Id = Guid.NewGuid(),
                FirearmId = firearm.Id,
                PreviousOwnerCitizenId = null,
                NewOwnerCitizenId = citizenProfile.Id,
                TransferType = TransferType.Sale,
                TransferDate = firearm.RegisteredAt,
                CreatedByUserId = shopUser.Id,
                Notes = "Initial purchase from authorized dealer",
                CreatedAt = DateTime.UtcNow
            };

            context.OwnershipHistories.Add(ownershipHistory);

            // Create sample audit logs
            var auditLogs = new[]
            {
                new AuditLog
                {
                    Id = Guid.NewGuid(),
                    UserId = adminUser.Id,
                    UserRole = "Admin",
                    Action = "System.Seed",
                    EntityType = "System",
                    TimestampUtc = DateTime.UtcNow,
                    Description = "Database seeded with test data"
                },
                new AuditLog
                {
                    Id = Guid.NewGuid(),
                    UserId = citizenUser.Id,
                    UserRole = "Citizen",
                    Action = "Login.Success",
                    TimestampUtc = DateTime.UtcNow.AddDays(-1),
                    IpAddress = "127.0.0.1",
                    Description = "User logged in successfully"
                },
                new AuditLog
                {
                    Id = Guid.NewGuid(),
                    UserId = shopUser.Id,
                    UserRole = "Shop",
                    Action = "ShopRegisterSale.Success",
                    EntityType = "Firearm",
                    EntityId = firearm.Id.ToString(),
                    TimestampUtc = firearm.RegisteredAt,
                    Description = "Firearm sale registered"
                }
            };

            context.AuditLogs.AddRange(auditLogs);

            await context.SaveChangesAsync();

            logger.LogInformation("Database seeded successfully");
            logger.LogInformation("Test users created:");
            logger.LogInformation("  - admin@example.com / Admin123! (Admin)");
            logger.LogInformation("  - officer@example.com / Officer123! (WpaOfficer)");
            logger.LogInformation("  - citizen@example.com / Citizen123! (Citizen)");
            logger.LogInformation("  - shop@example.com / Shop123! (Shop)");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error seeding database");
            throw;
        }
    }
}
