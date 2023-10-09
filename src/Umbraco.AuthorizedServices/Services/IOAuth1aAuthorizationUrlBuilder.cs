using Umbraco.AuthorizedServices.Configuration;

namespace Umbraco.AuthorizedServices.Services;

/// <summary>
/// Defines operations on building the authorization URLs for OAuth1a flows.
/// </summary>
public interface IOAuth1aAuthorizationUrlBuilder
{
    /// <summary>
    /// Builds the authorization URL.
    /// </summary>
    /// <param name="serviceDetail">The service detail.</param>
    /// <param name="path">The path returned by the service during the request token authorization step.</param>
    /// <returns></returns>
    string BuildAuthorizationUrl(ServiceDetail serviceDetail, string path);

    /// <summary>
    /// Builds the request token URL.
    /// </summary>
    /// <param name="serviceDetail">The service detail.</param>
    /// <param name="httpMethod">The HTTP method.</param>
    /// <returns></returns>
    string BuildRequestTokenUrl(ServiceDetail serviceDetail, HttpMethod httpMethod);
}
