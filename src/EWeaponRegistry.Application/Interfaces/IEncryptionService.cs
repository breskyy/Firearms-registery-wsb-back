namespace EWeaponRegistry.Application.Interfaces;

/// <summary>
/// Service for encrypting/decrypting sensitive data using AES-256.
/// Used for PESEL, personal data, and other sensitive information.
/// </summary>
public interface IEncryptionService
{
    string Encrypt(string plainText);
    string Decrypt(string cipherText);
    string EncryptDate(DateTime date);
    DateTime? DecryptDate(string? encryptedDate);
}
