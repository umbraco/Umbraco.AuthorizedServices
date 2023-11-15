using Umbraco.AuthorizedServices.Models;

namespace Umbraco.AuthorizedServices.Services;

/// <summary>
/// Defines operations for storing <see cref="OAuth2Token"/> or <see cref="OAuth1Token"/> instances.
/// </summary>
public interface ITokenStorage<T>
    where T : class
{
    /// <summary>
    /// Retrieves a stored token for a service.
    /// </summary>
    /// <param name="serviceAlias">The service alias.</param>
    /// <returns>The <see cref="OAuth2Token"/> or <see cref="OAuth1Token"/> instance (or null, if not found).</returns>
    Task<T?> GetTokenAsync(string serviceAlias);

    /// <summary>
    /// Stores a token for a service.
    /// </summary>
    /// <param name="serviceAlias">The service alias.</param>
    /// <param name="token">The <see cref="OAuth2Token"/> or <see cref="OAuth1Token"/>.</param>
    Task SaveTokenAsync(string serviceAlias, T token);

    /// <summary>
    /// Deletes a stored token.
    /// </summary>
    /// <param name="serviceAlias">The service alias.</param>
    Task DeleteTokenAsync(string serviceAlias);
}
