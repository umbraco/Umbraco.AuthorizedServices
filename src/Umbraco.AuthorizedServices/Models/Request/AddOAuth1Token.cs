namespace Umbraco.AuthorizedServices.Models.Request;

/// <summary>
/// Defines the model used for saving an OAuth1 service token and token secret via a backoffice operation.
/// </summary>
public class AddOAuth1Token
{
    /// <summary>
    /// Gets or sets the service alias.
    /// </summary>
    public string Alias { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the service access token.
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the service token secret.
    /// </summary>
    public string TokenSecret { get; set; } = string.Empty;
}
