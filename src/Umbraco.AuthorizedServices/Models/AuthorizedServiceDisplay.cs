using System.Runtime.Serialization;

namespace Umbraco.AuthorizedServices.Models;

/// <summary>
/// Defines the details of an authorized service for display in the backoffice.
/// </summary>
[DataContract(Name = "authorizedService", Namespace = "")]
public class AuthorizedServiceDisplay
{
    /// <summary>
    /// Gets or sets the service's display name..
    /// </summary>
    [DataMember(Name = "displayName")]
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the service is authorized.
    /// </summary>
    [DataMember(Name = "isAuthorized")]
    public bool IsAuthorized { get; set; }

    /// <summary>
    /// Get or sets a value indicating whether editor can manually add token.
    /// </summary>
    [DataMember(Name = "canManuallyProvideToken")]
    public bool CanManuallyProvideToken { get; set; }

    /// <summary>
    /// Gets or sets the service's authorization URL.
    /// </summary>
    [DataMember(Name = "authorizationUrl")]
    public string? AuthorizationUrl { get; set; }

    /// <summary>
    /// Gets or sets a sample GET request for the service, used for verification.
    /// </summary>
    [DataMember(Name = "sampleRequest")]
    public string? SampleRequest { get; set; }

    /// <summary>
    /// Gets or sets the service's settings.
    /// </summary>
    [DataMember(Name = "settings")]
    public Dictionary<string, string> Settings { get; set; } = new Dictionary<string, string>();
}
