using Umbraco.AuthorizedServices.Services;
using Umbraco.AuthorizedServices.Services.Implement;

namespace Umbraco.AuthorizedServices.Tests.Services;

internal class AesSecretEncryptorTests : EncryptorTestsBase
{
    protected override ISecretEncryptor CreateEncrytor()
    {
        const string SecretKey = "secret";
        return new AesSecretEncryptor(SecretKey);
    }
}
