namespace Umbraco.AuthorizedServices.Models.Request;

/// <summary>
/// Defines the model used for saving a service token via a backoffice operation.
/// </summary>
public class AddOAuth2Token
{
    /// <summary>
    /// Gets or sets the service alias.
    /// </summary>
    public string Alias { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the service token.
    /// </summary>
    public string Token { get; set; } = string.Empty;
}
