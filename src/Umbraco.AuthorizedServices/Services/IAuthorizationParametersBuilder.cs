using Umbraco.AuthorizedServices.Configuration;

namespace Umbraco.AuthorizedServices.Services;

/// <summary>
/// Defines operations on building the parameter dictionary used in authorization requests.
/// </summary>
public interface IAuthorizationParametersBuilder
{
    /// <summary>
    /// Builds the parameter dictionary used in OAuth2AuthorizationCode authorization requests.
    /// </summary>
    /// <param name="serviceDetail">The service detail.</param>
    /// <param name="authorizationCode">The authorization code.</param>
    /// <param name="redirectUri">The redirect URL.</param>
    /// <param name="codeVerifier">The code verifier that was hashed and sent as code challenge.</param>
    /// <returns>A dictionary containing the authorization parameters.</returns>
    Dictionary<string, string> BuildParametersForOAuth2AuthorizationCode(ServiceDetail serviceDetail, string authorizationCode, string redirectUri, string codeVerifier);

    /// <summary>
    /// Builds the parameter dictionary used in OAuth2ClientCredentials authorization requests.
    /// </summary>
    /// <param name="serviceDetail">The service detail.</param>
    /// <returns>A dictionary containing the authorization parameters.</returns>
    Dictionary<string, string> BuildParametersForOAuth2ClientCredentials(ServiceDetail serviceDetail);

    /// <summary>
    /// Builds the parameter dictionary used in OAuth1 authorization requests.
    /// </summary>
    /// <param name="serviceDetail">The service detail.</param>
    /// <param name="oauthToken">The OAuth Token.</param>
    /// <param name="oauthVerifier">The OAuth Verifier.</param>
    /// <param name="oauthTokenSecret">The OAuth Token Secret.</param>
    /// <returns>A dictionary containing the authorization parameters.</returns>
    Dictionary<string, string> BuildParametersForOAuth1(ServiceDetail serviceDetail, string oauthToken, string oauthVerifier, string oauthTokenSecret);
}
