using Microsoft.AspNetCore.DataProtection;
using Umbraco.AuthorizedServices.Services;
using Umbraco.AuthorizedServices.Services.Implement;

namespace Umbraco.AuthorizedServices.Tests.Services;

internal class DataProtectionSecretEncrytorTests : EncryptorTestsBase
{
    protected override ISecretEncryptor CreateEncrytor()
    {
        var dataProtectionProvider = new EphemeralDataProtectionProvider();
        return new DataProtectionSecretEncryptor(dataProtectionProvider);
    }
}
