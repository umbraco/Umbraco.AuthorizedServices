using Umbraco.AuthorizedServices.Configuration;
using Umbraco.AuthorizedServices.Models;

namespace Umbraco.AuthorizedServices.Services;

/// <summary>
/// Defines operations on building a request to an authorized service.
/// </summary>
public interface IAuthorizedRequestBuilder
{
    /// <summary>
    /// Creates an request to an authorized service.
    /// </summary>
    /// <typeparam name="TRequest">The typed request data.</typeparam>
    /// <param name="serviceDetail">The service detail.</param>
    /// <param name="path">The request path.</param>
    /// <param name="httpMethod">The HTTP method.</param>
    /// <param name="token">The authorization token.</param>
    /// <param name="requestContent">The request data.</param>
    /// <returns>The request instance.</returns>
    HttpRequestMessage CreateRequestMessageWithToken<TRequest>(
        ServiceDetail serviceDetail,
        string path,
        HttpMethod httpMethod,
        Token token,
        TRequest? requestContent)
        where TRequest : class;

    /// <summary>
    /// Creates an request to an authorized service using API key.
    /// </summary>
    /// <typeparam name="TRequest">The typed request data.</typeparam>
    /// <param name="serviceDetail">The service detail.</param>
    /// <param name="path">The request path.</param>
    /// <param name="httpMethod">The HTTP method.</param>
    /// <param name="apiKey">The authorization API key.</param>
    /// <param name="requestContent">The request data.</param>
    /// <returns>The request instance.</returns>
    HttpRequestMessage CreateRequestMessageWithApiKey<TRequest>(
        ServiceDetail serviceDetail,
        string path,
        HttpMethod httpMethod,
        string apiKey,
        TRequest? requestContent)
        where TRequest : class;
}
