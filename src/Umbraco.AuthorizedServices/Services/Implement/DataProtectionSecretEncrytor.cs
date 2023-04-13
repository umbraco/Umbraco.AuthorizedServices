using Microsoft.AspNetCore.DataProtection;

namespace Umbraco.AuthorizedServices.Services.Implement
{
    internal sealed class DataProtectionSecretEncrytor : ISecretEncryptor
    {
        public IDataProtector Protector { get; set; }

        private readonly IDataProtectionProvider _dataProtectionProvider;

        private const string Purpose = "UmbracoAuthorizedServiceTokens";

        public DataProtectionSecretEncrytor(IDataProtectionProvider dataProtectionProvider)
        {
            _dataProtectionProvider = dataProtectionProvider;

            Protector = _dataProtectionProvider.CreateProtector(Purpose);
        }

        public string Encrypt(string value) => Protector.Protect(value);

        public string Decrypt(string value) => Protector.Unprotect(value);
    }
}
