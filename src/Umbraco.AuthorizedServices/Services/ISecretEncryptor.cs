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
    /// <param name="value">The value to decrypt.</param>
    /// <returns>The decrypted value.</returns>
    string Decrypt(string value);
}
