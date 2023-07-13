namespace Umbraco.AuthorizedServices.Services;

/// <summary>
/// Defines operations on encrypting and decrypting values.
/// </summary>
public interface ISecretEncryptor
{
    /// <summary>
    /// Encrypts the provided value.
    /// </summary>
    /// <param name="value">The value to encrypt.</param>
    /// <returns>The encrypted value.</returns>
    string Encrypt(string value);

    /// <summary>
    /// Decrypts the provided value.
    /// </summary>
    /// <param name="encryptedValue">The value to decrypt.</param>
    /// <param name="decryptedValue">Resulting decrypted value.</param>
    /// <returns>True if the value could be decrypted, false if not.</returns>
    bool TryDecrypt(string encryptedValue, out string decryptedValue);
}
