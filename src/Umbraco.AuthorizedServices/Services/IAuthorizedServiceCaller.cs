namespace Umbraco.AuthorizedServices.Services;

/// <summary>
/// Defines operations on making requests to an external service.
/// </summary>
public interface IAuthorizedServiceCaller
{
    /// <summary>
    /// Sends a request to an authorized service expecting no response.
    /// </summary>
    /// <param name="serviceAlias">The service alias.</param>
    /// <param name="path">The request path.</param>
    /// <param name="httpMethod">The HTTP method.</param>
    Task SendRequestAsync(string serviceAlias, string path, HttpMethod httpMethod);

    /// <summary>
    /// Sends a request to an authorized service to receive a deserialized, strongly typed response.
    /// </summary>
    /// <typeparam name="TResponse">The response type.</typeparam>
    /// <param name="serviceAlias">The service alias.</param>
    /// <param name="path">The request path.</param>
    /// <param name="httpMethod">The HTTP method.</param>
    /// <returns>A <see cref="Task{TResponse}"/> representing the result of the asynchronous operation.</returns>
    Task<TResponse?> SendRequestAsync<TResponse>(string serviceAlias, string path, HttpMethod httpMethod);

    /// <summary>
    /// Sends a request with data to an authorized service to receive a deserialized, strongly typed response.
    /// </summary>
    /// <typeparam name="TResponse">The response type.</typeparam>
    /// <typeparam name="TRequest">The request type.</typeparam>
    /// <param name="serviceAlias">The service alias.</param>
    /// <param name="path">The request path.</param>
    /// <param name="httpMethod">The HTTP method.</param>
    /// <param name="requestContent">The request data.</param>
    /// <returns>A <see cref="Task{TResponse}"/> representing the result of the asynchronous operation.</returns>
    Task<TResponse?> SendRequestAsync<TRequest, TResponse>(string serviceAlias, string path, HttpMethod httpMethod, TRequest? requestContent = null)
        where TRequest : class;

    /// <summary>
    /// Sends a request to an authorized service to receive a raw string response.
    /// </summary>
    /// <param name="serviceAlias">The service alias.</param>
    /// <param name="path">The request path.</param>
    /// <param name="httpMethod">The HTTP method.</param>
    /// <returns>A <see cref="Task{String}"/> representing the result of the asynchronous operation.</returns>
    Task<string> SendRequestRawAsync(string serviceAlias, string path, HttpMethod httpMethod);

    /// <summary>
    /// Sends a request with data to an authorized service to receive a raw string response.
    /// </summary>
    /// <typeparam name="TRequest">The request type.</typeparam>
    /// <param name="serviceAlias">The service alias.</param>
    /// <param name="path">The request path.</param>
    /// <param name="httpMethod">The HTTP method.</param>
    /// <param name="requestContent">The request data.</param>
    /// <returns>A <see cref="Task{String}"/> representing the result of the asynchronous operation.</returns>

    Task<string> SendRequestRawAsync<TRequest>(string serviceAlias, string path, HttpMethod httpMethod, TRequest? requestContent = null)
        where TRequest : class;
}
