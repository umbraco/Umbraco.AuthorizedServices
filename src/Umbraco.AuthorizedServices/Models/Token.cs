namespace Umbraco.AuthorizedServices.Models;

/// <summary>
/// Defines a token used for authorizing requests to a service.
/// </summary>
public class Token
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Token"/> class.
    /// </summary>
    /// <param name="accessToken">The access token.</param>
    public Token(string accessToken) => AccessToken = accessToken;

    /// <summary>
    /// Initializes a new instance of the <see cref="Token"/> class.
    /// </summary>
    /// <param name="accessToken">The access token.</param>
    /// <param name="refreshToken">The refreh token.</param>
    /// <param name="expiresOn">The date the access token expires.</param>
    public Token(string accessToken, string? refreshToken, DateTime? expiresOn)
        : this(accessToken)
    {
        RefreshToken = refreshToken;
        ExpiresOn = expiresOn;
    }

    /// <summary>
    /// Gets or sets the access token.
    /// </summary>
    public string AccessToken { get; }

    /// <summary>
    /// Gets or sets the refresh token.
    /// </summary>
    public string? RefreshToken { get; }

    /// <summary>
    /// Gets or sets the date the access token expires.
    /// </summary>
    public DateTime? ExpiresOn { get; }

    /// <summary>
    /// Checks to see if the token either has or is about to expire (in the next 30 seconds).
    /// </summary>
    public bool HasOrIsAboutToExpire => ExpiresOn.HasValue && DateTime.UtcNow.AddSeconds(30) > ExpiresOn;
}
