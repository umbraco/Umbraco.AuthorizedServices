namespace Umbraco.AuthorizedServices.Services;

public interface ISecretEncryptor
{
    string Encrypt(string value);

    string Decrypt(string value);
}
