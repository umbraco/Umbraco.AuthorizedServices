namespace Umbraco.AuthorizedServices.Services.Implement;

internal sealed class NoopSecretEncryptor : ISecretEncryptor
{
    public string Encrypt(string value) => value;

    public bool TryDecrypt(string encryptedValue, out string decryptedValue)
    {
        decryptedValue = encryptedValue;
        return true;
    }
}
