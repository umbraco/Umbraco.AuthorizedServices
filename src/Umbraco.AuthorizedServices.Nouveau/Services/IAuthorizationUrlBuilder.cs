using Microsoft.AspNetCore.Http;
using Umbraco.AuthorizedServices.Configuration;

namespace Umbraco.AuthorizedServices.Services;

/// <summary>
/// Defines operations on building an authorization URL.
/// </summary>
public interface IAuthorizationUrlBuilder
{
    /// <summary>
    /// Builds the OAuth1 request token URL.
    /// </summary>
    /// <param name="serviceDetail">The service detail.</param>
    /// <param name="httpContext">The HTTP context.</param>
    /// <param name="httpMethod">The HTTP method.</param>
    /// <param name="nonce">The randomly generated string.</param>
    /// <param name="timestamp"> The current time in seconds.</param>
    /// <returns></returns>
    string BuildOAuth1RequestTokenUrl(ServiceDetail serviceDetail, HttpContext? httpContext, HttpMethod httpMethod, string nonce, string timestamp);

    /// <summary>
    /// Builds the OAuth2 authorization URL.
    /// </summary>
    /// <param name="serviceDetail">The service detail.</param>
    /// <param name="httpContext">The HTTP context.</param>
    /// <param name="state">The randomly generated state.</param>
    /// <param name="codeChallenge">Code verifier hash used for PKCE OAuth flows.</param>
    /// <returns>The authorization URL.</returns>
    string BuildOAuth2AuthorizationUrl(ServiceDetail serviceDetail, HttpContext httpContext, string state, string codeChallenge);
}
