using System.Security.Cryptography;
using System.Text;
using EWeaponRegistry.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace EWeaponRegistry.Infrastructure.Services;

/// <summary>
/// AES-256-CBC encryption service for sensitive data.
/// Key must be provided via configuration/environment variables.
/// NEVER hardcode the encryption key in source code.
/// </summary>
public class EncryptionService : IEncryptionService
{
    private readonly byte[] _key;
    private const int IvSize = 16;

    public EncryptionService(IConfiguration configuration)
    {
        var keyString = configuration["Encryption:Key"]
            ?? throw new InvalidOperationException("Encryption key not configured. Set Encryption:Key in configuration.");

        if (keyString.Length != 32)
        {
            throw new InvalidOperationException("Encryption key must be exactly 32 characters (256 bits) for AES-256.");
        }

        _key = Encoding.UTF8.GetBytes(keyString);
    }

    public string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
            return string.Empty;

        using var aes = Aes.Create();
        aes.Key = _key;
        aes.GenerateIV();
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        using var encryptor = aes.CreateEncryptor();
        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

        // Prepend IV to cipher text for decryption
        var result = new byte[IvSize + cipherBytes.Length];
        Buffer.BlockCopy(aes.IV, 0, result, 0, IvSize);
        Buffer.BlockCopy(cipherBytes, 0, result, IvSize, cipherBytes.Length);

        return Convert.ToBase64String(result);
    }

    public string Decrypt(string cipherText)
    {
        if (string.IsNullOrEmpty(cipherText))
            return string.Empty;

        var fullCipher = Convert.FromBase64String(cipherText);

        using var aes = Aes.Create();
        aes.Key = _key;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        // Extract IV from the beginning
        var iv = new byte[IvSize];
        Buffer.BlockCopy(fullCipher, 0, iv, 0, IvSize);
        aes.IV = iv;

        var cipherBytes = new byte[fullCipher.Length - IvSize];
        Buffer.BlockCopy(fullCipher, IvSize, cipherBytes, 0, cipherBytes.Length);

        using var decryptor = aes.CreateDecryptor();
        var plainBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);

        return Encoding.UTF8.GetString(plainBytes);
    }

    public string EncryptDate(DateTime date)
    {
        return Encrypt(date.ToString("O"));
    }

    public DateTime? DecryptDate(string? encryptedDate)
    {
        if (string.IsNullOrEmpty(encryptedDate))
            return null;

        var decrypted = Decrypt(encryptedDate);
        return DateTime.TryParse(decrypted, out var date) ? date : null;
    }
}
