using Umbraco.AuthorizedServices.Models;

namespace Umbraco.AuthorizedServices.Services;

public interface IAuthorizationPayloadBuilder
{
    /// <summary>
    /// Builds the authorization payload used in authorization requests.
    /// </summary>
    /// <returns>An AuthorizedServiceAuthorizationPayload instance.</returns>
    AuthorizationPayload BuildPayload();
}
