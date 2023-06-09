using Microsoft.AspNetCore.DataProtection;

namespace Umbraco.AuthorizedServices.Services.Implement;

internal sealed class DataProtectionSecretEncrytor : ISecretEncryptor
{
    private readonly IDataProtector _protector;

    private const string Purpose = "UmbracoAuthorizedServiceTokens";

    public DataProtectionSecretEncrytor(IDataProtectionProvider dataProtectionProvider)
    {
        _protector = dataProtectionProvider.CreateProtector(Purpose);
    }

    public string Encrypt(string value) => _protector.Protect(value);

    public string Decrypt(string value) => _protector.Unprotect(value);
}
