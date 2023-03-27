using Microsoft.AspNetCore.Http;
using Umbraco.AuthorizedServices.Configuration;

namespace Umbraco.AuthorizedServices.Services;

/// <summary>
/// Defines operations on building an authorization URL.
/// </summary>
public interface IAuthorizationUrlBuilder
{
    /// <summary>
    /// Buulds the authorization URL.
    /// </summary>
    /// <param name="serviceDetail">The service detail.</param>
    /// <param name="httpContext">The HTTP context.</param>
    /// <returns>The authorization URL.</returns>
    string BuildUrl(ServiceDetail serviceDetail, HttpContext httpContext);
}
