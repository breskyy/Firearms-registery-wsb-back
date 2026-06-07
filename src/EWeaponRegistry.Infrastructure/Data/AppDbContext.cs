using EWeaponRegistry.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EWeaponRegistry.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<CitizenProfile> CitizenProfiles => Set<CitizenProfile>();
    public DbSet<Permit> Permits => Set<Permit>();
    public DbSet<Firearm> Firearms => Set<Firearm>();
    public DbSet<OwnershipHistory> OwnershipHistories => Set<OwnershipHistory>();
    public DbSet<Promise> Promises => Set<Promise>();
    public DbSet<PromiseApplication> PromiseApplications => Set<PromiseApplication>();
    public DbSet<TransferRequest> TransferRequests => Set<TransferRequest>();
    public DbSet<Shop> Shops => Set<Shop>();
    public DbSet<MedicalAlert> MedicalAlerts => Set<MedicalAlert>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<PermitApplication> PermitApplications => Set<PermitApplication>();
    public DbSet<PermitApplicationAttachment> PermitApplicationAttachments => Set<PermitApplicationAttachment>();
    public DbSet<PromiseApplicationAttachment> PromiseApplicationAttachments => Set<PromiseApplicationAttachment>();
    public DbSet<PermitMedicalExamRenewal> PermitMedicalExamRenewals => Set<PermitMedicalExamRenewal>();
    public DbSet<PermitMedicalExamRenewalAttachment> PermitMedicalExamRenewalAttachments => Set<PermitMedicalExamRenewalAttachment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all configurations from current assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
