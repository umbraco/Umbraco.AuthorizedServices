using Umbraco.AuthorizedServices.Models;

namespace Umbraco.AuthorizedServices.Services;

/// <summary>
/// Defines operations for storing <see cref="Token"/> or <see cref="OAuth1aToken"/> instances.
/// </summary>
public interface ITokenStorage<T>
    where T : class
{
    /// <summary>
    /// Retrieves a stored token for a service.
    /// </summary>
    /// <param name="serviceAlias">The service alias.</param>
    /// <returns>The <see cref="Token"/> or <see cref="OAuth1aToken"/> instance (or null, if not found).</returns>
    T? GetToken(string serviceAlias);

    /// <summary>
    /// Stores a token for a service.
    /// </summary>
    /// <param name="serviceAlias">The service alias.</param>
    /// <param name="token">The <see cref="Token"/> or <see cref="OAuth1aToken"/>.</param>
    void SaveToken(string serviceAlias, T token);

    /// <summary>
    /// Deletes a stored token.
    /// </summary>
    /// <param name="serviceAlias">The service alias.</param>
    void DeleteToken(string serviceAlias);
}
