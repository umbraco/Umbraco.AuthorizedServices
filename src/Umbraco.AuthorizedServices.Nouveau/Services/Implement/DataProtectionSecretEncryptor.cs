using System.Security.Cryptography;
using Microsoft.AspNetCore.DataProtection;

namespace Umbraco.AuthorizedServices.Services.Implement;

internal sealed class DataProtectionSecretEncryptor : ISecretEncryptor
{
    private readonly IDataProtector _protector;

    private const string Purpose = "UmbracoAuthorizedServiceTokens";

    public DataProtectionSecretEncryptor(IDataProtectionProvider dataProtectionProvider) =>
        _protector = dataProtectionProvider.CreateProtector(Purpose);

    public string Encrypt(string value) => _protector.Protect(value);

    public bool TryDecrypt(string encryptedValue, out string decryptedValue)
    {
        try
        {
            decryptedValue = _protector.Unprotect(encryptedValue);
            return true;
        }
        catch (CryptographicException)
        {
            decryptedValue = string.Empty;
            return false;
        }
    }
}
