using Umbraco.AuthorizedServices.Models;

namespace Umbraco.AuthorizedServices.Services;

/// <summary>
/// Defines operations on authorizing with an external service.
/// </summary>
public interface IAuthorizedServiceAuthorizer
{
    /// <summary>
    /// Authorizises access to an external service.
    /// </summary>
    /// <param name="serviceAlias">The service alias.</param>
    /// <param name="authorizationCode">The authorization code.</param>
    /// <param name="redirectUri">The redirect URL.</param>
    /// <returns>A <see cref="Task{AuthorizationResult}"/> representing the result of the asynchronous operation.</returns>
    Task<AuthorizationResult> AuthorizeServiceAsync(string serviceAlias, string authorizationCode, string redirectUri);
}
