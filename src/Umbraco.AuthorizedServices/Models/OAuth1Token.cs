namespace Umbraco.AuthorizedServices.Models;

/// <summary>
/// Defines a token used for authorizing OAuth1 requests to a service.
/// </summary>
public class OAuth1Token
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OAuth1Token"/> class.
    /// </summary>
    /// <param name="oauthToken">The access token.</param>
    /// <param name="oauthTokenSecret">The token secret.</param>
    public OAuth1Token(string oauthToken, string oauthTokenSecret)
    {
        OAuthToken = oauthToken;
        OAuthTokenSecret = oauthTokenSecret;
    }

    /// <summary>
    /// Gets or sets the OAuth token.
    /// </summary>
    public string OAuthToken { get; set; }

    /// <summary>
    /// Gets or sets the OAuth token secret.
    /// </summary>
    public string OAuthTokenSecret { get; set; }
}
