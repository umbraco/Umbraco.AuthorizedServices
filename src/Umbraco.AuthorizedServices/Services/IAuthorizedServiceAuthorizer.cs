using Umbraco.AuthorizedServices.Models;

namespace Umbraco.AuthorizedServices.Services;

public interface IAuthorizedServiceAuthorizer
{
    Task<AuthorizationResult> AuthorizeServiceAsync(string serviceAlias, string authorizationCode, string redirectUri);
}
