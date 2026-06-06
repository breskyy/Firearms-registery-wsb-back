namespace EWeaponRegistry.Domain.Constants;

public static class ApplicationPaymentBankDetails
{
    public const string AccountNumber = "12 3456 7890 1234 5678 9012 3456";
    public const string AccountHolder = "Urząd Miasta — Wydział Policji Administracyjnej (WPA)";
    public const string BankName = "Bank Demo S.A.";

    public static string BuildTransferTitle(Guid applicationId, string applicationKind) =>
        applicationKind switch
        {
            "permit" => $"Opłata skarbowa — wniosek o pozwolenie {applicationId:N}",
            "promise" => $"Opłata skarbowa — promesa {applicationId:N}",
            _ => $"Opłata skarbowa {applicationId:N}"
        };
}
