using Umbraco.AuthorizedServices.Configuration;

namespace Umbraco.AuthorizedServices.Services;

/// <summary>
/// Defines operations on building the parameter dictionary used in authorization requests.
/// </summary>
public interface IAuthorizationParametersBuilder
{
    /// <summary>
    /// Builds the the parameter dictionary used in authorization requests.
    /// </summary>
    /// <param name="serviceDetail">The service detail.</param>
    /// <param name="authorizationCode">The authorization code.</param>
    /// <param name="redirectUri">The redirect URL.</param>
    /// <returns>A dictionary containing the authorization parameters.</returns>
    Dictionary<string, string> BuildParameters(ServiceDetail serviceDetail, string authorizationCode, string redirectUri);
}
