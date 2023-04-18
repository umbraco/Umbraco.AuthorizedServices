namespace Umbraco.AuthorizedServices.Models;

/// <summary>
/// Defines a payload object used in the authorization flow.
/// </summary>
public class AuthorizedServiceAuthorizationPayload
{
    /// <summary>
    /// Gets or sets a random state parameter sent to the authorization server.
    /// </summary>
    public string State { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a string value used for OAuth with PKCE flows. Sent to the authorization server when requesting an access token.
    /// </summary>
    public string CodeVerifier { get; set; } = string.Empty;

    /// <summary>
    /// Hashed value of the code verifier for OAuth with PKCE flows. Sent as a query string parameter in the authorization URL.
    /// </summary>
    public string CodeChallenge { get; set; } = string.Empty;
}
