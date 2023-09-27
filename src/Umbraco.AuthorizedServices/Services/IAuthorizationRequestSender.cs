using Umbraco.AuthorizedServices.Configuration;

namespace Umbraco.AuthorizedServices.Services;

/// <summary>
/// Defines operations on sending requests for authorization with a service.
/// </summary>
public interface IAuthorizationRequestSender
{
    /// <summary>
    /// Sends an authorization request to a service.
    /// </summary>
    /// <param name="serviceDetail">The service detail.</param>
    /// <param name="parameters">The authorization parameters.</param>
    /// <returns>A <see cref="Task{HttpResponseMessage}"/> representing the result of the asynchronous operation.</returns>
    Task<HttpResponseMessage> SendRequest(ServiceDetail serviceDetail, Dictionary<string, string> parameters);

    /// <summary>
    /// Sends an authorization request to a service.
    /// </summary>
    /// <param name="serviceDetail">The service detail.</param>
    /// <param name="parameters">The authorization parameters.</param>
    /// <returns>A <see cref="Task{HttpResponseMessage}"/> representing the result of the asynchronous operation.</returns>
    Task<HttpResponseMessage> SendExchangeRequest(ServiceDetail serviceDetail, Dictionary<string, string> parameters);
}
