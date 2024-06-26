using Umbraco.AuthorizedServices.Configuration;
using Umbraco.AuthorizedServices.Models;

namespace Umbraco.AuthorizedServices.Services;

/// <summary>
/// Defines operations on creating <see cref="OAuth2Token"/> instances.
/// </summary>
public interface ITokenFactory
{
    /// <summary>
    /// Creates a <see cref="OAuth2Token"/> instance from an authorization response.
    /// </summary>
    /// <param name="responseContent">The response content.</param>
    /// <param name="serviceDetail">The service detail.</param>
    /// <returns>The <see cref="OAuth2Token"/> instance.</returns>
    OAuth2Token CreateFromOAuth2ResponseContent(string responseContent, ServiceDetail serviceDetail);

    /// <summary>
    /// Creates a <see cref="OAuth1Token"/> instance from an authorization response.
    /// </summary>
    /// <param name="responseContent">The response content.</param>
    /// <returns>The <see cref="OAuth1Token"/> instance.</returns>
    OAuth1Token CreateFromOAuth1ResponseContent(string responseContent);
}
