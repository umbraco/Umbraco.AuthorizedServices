namespace Umbraco.AuthorizedServices.Services.Implement;

internal sealed class NoopSecretEncryptor : ISecretEncryptor
{
    public string Encrypt(string value) => value;

    public string Decrypt(string value) => value;
}
