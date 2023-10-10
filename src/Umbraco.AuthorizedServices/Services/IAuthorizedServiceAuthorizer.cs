using Umbraco.AuthorizedServices.Models;

namespace Umbraco.AuthorizedServices.Services;

/// <summary>
/// Defines operations on authorizing with an external service.
/// </summary>
public interface IAuthorizedServiceAuthorizer
{
    /// <summary>
    /// Authorizes access to an external service using OAuth2AuthorizationCode authentication method.
    /// </summary>
    /// <param name="serviceAlias">The service alias.</param>
    /// <param name="authorizationCode">The authorization code.</param>
    /// <param name="redirectUri">The redirect URL.</param>
    /// <param name="codeVerifier">The string used to validate requests for OAuth with PKCE flows.</param>
    /// <returns>A <see cref="Task{AuthorizationResult}"/> representing the result of the asynchronous operation.</returns>
    Task<AuthorizationResult> AuthorizeOAuth2AuthorizationCodeServiceAsync(string serviceAlias, string authorizationCode, string redirectUri, string codeVerifier);

    /// <summary>
    /// Authorizes access to an external service using OAuth2ClientCredentials authentication method.
    /// </summary>
    /// <param name="serviceAlias">The service alias.</param>
    /// <returns>A <see cref="Task{AuthorizationResult}"/> representing the result of the asynchronous operation.</returns>
    Task<AuthorizationResult> AuthorizeOAuth2ClientCredentialsServiceAsync(string serviceAlias);

    /// <summary>
    /// Exchanges the access token with a long lived one.
    /// </summary>
    /// <param name="serviceAlias">The service alias.</param>
    /// <returns>A <see cref="Task{AuthorizationResult}"/> representing the result of the asynchronous operation.</returns>
    Task<AuthorizationResult> ExchangeOAuth2AccessTokenAsync(string serviceAlias);

    /// <summary>
    /// Authorizes access to an external service using OAuth1 authentication flow.
    /// </summary>
    /// <param name="serviceAlias">The service alias.</param>
    /// <param name="oauthToken">The returned OAuth token.</param>
    /// <param name="oauthVerifier">The returned OAuth verifier code.</param>
    /// <returns>A <see cref="Task{AuthorizationResult}"/> representing the result of the asynchronous operation.</returns>
    Task<AuthorizationResult> AuthorizeOAuth1ServiceAsync(string serviceAlias, string oauthToken, string oauthVerifier);
}
