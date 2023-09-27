using Umbraco.AuthorizedServices.Configuration;

namespace Umbraco.AuthorizedServices.Services;

/// <summary>
/// Defines operations on building the parameter dictionary used in token refresh requests.
/// </summary>
public interface IRefreshTokenParametersBuilder
{
    /// <summary>
    /// Builds the parameter dictionary used in token refresh requests.
    /// </summary>
    /// <param name="serviceDetail">The service detail.</param>
    /// <param name="refreshToken">The refresh token.</param>
    /// <returns>A dictionary containing the refresh token parameters.</returns>
    Dictionary<string, string> BuildParameters(ServiceDetail serviceDetail, string refreshToken);
}
