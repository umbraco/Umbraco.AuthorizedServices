using System.Runtime.Serialization;

namespace Umbraco.AuthorizedServices.Models;

[DataContract(Name = "authorizedService", Namespace = "")]
public class AuthorizedServiceDisplay
{
    [DataMember(Name = "displayName")]
    public string DisplayName { get; set; } = string.Empty;

    [DataMember(Name = "isAuthorized")]
    public bool IsAuthorized { get; set; }

    [DataMember(Name = "authorizationUrl")]
    public string? AuthorizationUrl { get; set; }

    [DataMember(Name = "sampleRequest")]
    public string? SampleRequest { get; set; }
}
