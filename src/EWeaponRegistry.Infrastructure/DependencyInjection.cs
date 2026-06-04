using EWeaponRegistry.Application.Interfaces;
using EWeaponRegistry.Application.Interfaces.ExternalGateways;
using EWeaponRegistry.Infrastructure.Data;
using EWeaponRegistry.Infrastructure.ExternalGateways;
using EWeaponRegistry.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EWeaponRegistry.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Database
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        // Services
        services.AddScoped<IEncryptionService, EncryptionService>();
        services.AddScoped<IAuditService, AuditService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ICitizenService, CitizenService>();
        services.AddScoped<IShopService, ShopService>();
        services.AddScoped<IWpaService, WpaService>();
        services.AddScoped<IAdminService, AdminService>();
        services.AddScoped<IPermitMedicalExamRenewalService, PermitMedicalExamRenewalService>();

        // External Gateways (Mock implementations)
        services.AddSingleton<INationalLoginGateway, MockNationalLoginGateway>();
        services.AddSingleton<IMObywatelGateway, MockMObywatelGateway>();
        services.AddSingleton<IPaymentGateway, MockPaymentGateway>();
        services.AddSingleton<IWpaExternalRegistryGateway, MockWpaExternalRegistryGateway>();
        services.AddSingleton<IPushNotificationGateway, MockPushNotificationGateway>();

        return services;
    }
}
