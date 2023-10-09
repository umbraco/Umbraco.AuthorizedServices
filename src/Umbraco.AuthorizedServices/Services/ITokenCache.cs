namespace Umbraco.AuthorizedServices.Services;

/// <summary>
/// Defines operations for storing temporarly an access token for OAuth1 flows, for a service.
/// </summary>
public interface ITokenCache
{
    /// <summary>
    /// Retrieves the cached token for a service.
    /// </summary>
    /// <param name="serviceAlias">The service alias.</param>
    /// <returns>The OAuth token (or null, if not found).</returns>
    string? Get(string serviceAlias);

    /// <summary>
    /// Retrieves the service alias by the cached OAuth token.
    /// </summary>
    /// <param name="token">The OAuth token.</param>
    /// <returns>The service alias matching the cached OAuth token (or null, if not found).</returns>
    string? GetByValue(string token);

    /// <summary>
    /// Stores an OAuth token for a service.
    /// </summary>
    /// <param name="serviceAlias">The service alias.</param>
    /// <param name="token">The OAuth token.</param>
    void Save(string serviceAlias, string token);

    /// <summary>
    /// Deletes a stored token.
    /// </summary>
    /// <param name="serviceAlias">The service alias.</param>
    void Delete(string serviceAlias);
}
