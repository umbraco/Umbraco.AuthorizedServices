using Umbraco.AuthorizedServices.Models;

namespace Umbraco.AuthorizedServices.Services;

public interface IAuthorizedServiceAuthorizationPayloadBuilder
{
    /// <summary>
    /// Builds the authorization payload used in authorization requests.
    /// </summary>
    /// <returns>An AuthorizedServiceAuthorizationPayload instance.</returns>
    AuthorizedServiceAuthorizationPayload BuildPayload();
}
