using Umbraco.AuthorizedServices.Configuration;
using Umbraco.AuthorizedServices.Models;

namespace Umbraco.AuthorizedServices.Services;

/// <summary>
/// Defines operations on creating <see cref="Token"/> instances.
/// </summary>
public interface ITokenFactory
{
    /// <summary>
    /// Creates a <see cref="Token"/> instance from an authorization response.
    /// </summary>
    /// <param name="responseContent">The response content.</param>
    /// <param name="serviceDetail">The service detail.</param>
    /// <returns>The <see cref="Token"/> instance.</returns>
    Token CreateFromResponseContent(string responseContent, ServiceDetail serviceDetail);
}
