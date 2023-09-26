namespace Umbraco.AuthorizedServices.Models.Request;

/// <summary>
/// Defines the model used for saving a service API key via a backoffice operation.
/// </summary>
public class AddApiKey
{
    /// <summary>
    /// Gets or sets the service alias.
    /// </summary>
    public string Alias { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the service API key.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;
}
