namespace Umbraco.AuthorizedServices.Models;

/// <summary>
/// Defines a token used for authorizing OAuth1a requests to a service.
/// </summary>
public class OAuth1aToken
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OAuth1aToken"/> class.
    /// </summary>
    /// <param name="oauthToken">The access token.</param>
    /// <param name="oauthTokenSecret">The token secret.</param>
    public OAuth1aToken(string oauthToken, string oauthTokenSecret)
    {
        OAuthToken = oauthToken;
        OAuthTokenSecret = oauthTokenSecret;
    }

    public string OAuthToken { get; set; }

    public string OAuthTokenSecret { get; set; }
}
