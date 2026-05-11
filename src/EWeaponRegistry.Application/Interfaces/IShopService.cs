using EWeaponRegistry.Application.DTOs.Shop;

namespace EWeaponRegistry.Application.Interfaces;

public interface IShopService
{
    Task<VerifyPermitResponse> VerifyPermitAsync(Guid shopUserId, VerifyPermitRequest request);
    Task<RegisterSaleResponse> RegisterSaleAsync(Guid shopUserId, RegisterSaleRequest request);
}
