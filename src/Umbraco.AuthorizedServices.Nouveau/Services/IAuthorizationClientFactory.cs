namespace Umbraco.AuthorizedServices.Services;

/// <summary>
/// Defines operations on creating authorization HTTP clients.
/// </summary>
public interface IAuthorizationClientFactory
{
    /// <summary>
    /// Creates an HTTP client for use in authorization operations.
    /// </summary>
    HttpClient CreateClient();
}
