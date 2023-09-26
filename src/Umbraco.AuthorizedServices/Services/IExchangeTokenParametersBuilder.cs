using Umbraco.AuthorizedServices.Configuration;

namespace Umbraco.AuthorizedServices.Services;

/// <summary>
/// Defines operations on building the parameter dictionary used in exchange token authorization requests.
/// </summary>
public interface IExchangeTokenParametersBuilder
{
    /// <summary>
    /// Builds the parameter dictionary used in the exchange token flow.
    /// </summary>
    /// <param name="serviceDetail">The service detail.</param>
    /// <param name="accessToken">The short lived access token.</param>
    /// <returns>A dictionary containing the authorization parameters.</returns>
    Dictionary<string, string> BuildParameters(ServiceDetail serviceDetail, string accessToken);
}
