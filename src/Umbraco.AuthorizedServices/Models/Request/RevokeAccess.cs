namespace Umbraco.AuthorizedServices.Models.Request;

/// <summary>
/// Defines the model used for revoking access to a service via a backoffice operation.
/// </summary>
public class RevokeAccess
{
    /// <summary>
    /// Gets or sets the service alias.
    /// </summary>
    public string Alias { get; set; } = string.Empty;
}
