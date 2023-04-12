using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Options;

using Umbraco.AuthorizedServices.Configuration;

namespace Umbraco.AuthorizedServices.Services.Implement
{
    internal sealed class DataProtectionSecretEncrytor : ISecretEncryptor
    {
        private readonly AuthorizedServiceSettings _settings;

        private readonly IDataProtectionProvider _dataProtectionProvider;

        private readonly IDataProtector _protector;

        private const string Purpose = "UmbracoAuthorizedServiceTokens";

        public DataProtectionSecretEncrytor(IOptions<AuthorizedServiceSettings> options, IDataProtectionProvider dataProtectionProvider)
        {
            _settings = options.Value;

            _dataProtectionProvider = dataProtectionProvider;

            _protector = _dataProtectionProvider.CreateProtector(Purpose);
        }

        public string Encrypt(string value)
        {
            if (string.IsNullOrEmpty(_settings.TokenEncryptionKey))
            {
                throw new ArgumentException("No secret key has been configured.", nameof(AuthorizedServiceSettings.TokenEncryptionKey));
            }

            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentException("The value to be encrypted cannot be empty.", nameof(value));
            }

            return _protector.Protect(value);

        }

        public string Decrypt(string value)
        {
            if (string.IsNullOrEmpty(_settings.TokenEncryptionKey))
            {
                throw new ArgumentException("No secret key has been configured.", nameof(AuthorizedServiceSettings.TokenEncryptionKey));
            }

            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentException("The value to be decrypted cannot be empty.", nameof(value));
            }

            return _protector.Unprotect(value);
        }
    }
}
