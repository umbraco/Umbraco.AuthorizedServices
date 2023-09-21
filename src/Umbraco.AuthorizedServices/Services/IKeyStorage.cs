namespace Umbraco.AuthorizedServices.Services;

/// <summary>
/// Defines operations for storing API keys.
/// </summary>
public interface IKeyStorage
{
    /// <summary>
    /// Retrieves a stored API key for a service.
    /// </summary>
    /// <param name="serviceAlias">The service alias.</param>
    /// <returns>The key value (or null, if not found).</returns>
    string? GetKey(string serviceAlias);

    /// <summary>
    /// Stores an API key for aa service.
    /// </summary>
    /// <param name="serviceAlias">The service alias.</param>
    /// <param name="key">The API key.</param>
    void SaveKey(string serviceAlias, string key);

    /// <summary>
    /// Deletes a stored API key.
    /// </summary>
    /// <param name="serviceAlias">The service alias.</param>
    void DeleteKey(string serviceAlias);
}
