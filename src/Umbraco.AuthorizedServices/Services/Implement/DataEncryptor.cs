using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Options;

using Umbraco.AuthorizedServices.Configuration;

namespace Umbraco.AuthorizedServices.Services.Implement
{
    internal sealed class DataEncryptor : ISecretEncryptor
    {
        private readonly AuthorizedServiceSettings _settings;

        private readonly IDataProtectionProvider _dataProtectionProvider;

        public DataEncryptor(IOptions<AuthorizedServiceSettings> options, IDataProtectionProvider dataProtectionProvider)
        {
            _settings = options.Value;

            _dataProtectionProvider = dataProtectionProvider;
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

            IDataProtector protector = _dataProtectionProvider.CreateProtector(_settings.TokenEncryptionKey);

            return protector.Protect(value);

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

            IDataProtector protector = _dataProtectionProvider.CreateProtector(_settings.TokenEncryptionKey);

            return protector.Unprotect(value);
        }
    }
}
