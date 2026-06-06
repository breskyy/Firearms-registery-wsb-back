namespace EWeaponRegistry.Domain.Constants;

public static class ApplicationPaymentFees
{
    public const decimal PermitApplicationFee = 242m;
    public const decimal PromiseFeePerCertificate = 17m;

    public static decimal CalculatePromiseFee(int quantity) =>
        PromiseFeePerCertificate * quantity;
}
