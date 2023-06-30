using Umbraco.AuthorizedServices.Services;
using Umbraco.AuthorizedServices.Services.Implement;

namespace Umbraco.AuthorizedServices.Tests.Services;

internal class NoopSecretEncrytorTests : EncryptorTestsBase
{
    protected override ISecretEncryptor CreateEncrytor() => new NoopSecretEncryptor();
}
