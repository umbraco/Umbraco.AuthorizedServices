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
    Task<HttpResponseMessage> SendOAuth2Request(ServiceDetail serviceDetail, Dictionary<string, string> parameters);

    /// <summary>
    /// Sends an authorization request to a service.
    /// </summary>
    /// <param name="serviceDetail">The service detail.</param>
    /// <param name="parameters">The authorization parameters.</param>
    /// <returns>A <see cref="Task{HttpResponseMessage}"/> representing the result of the asynchronous operation.</returns>
    Task<HttpResponseMessage> SendOAuth2ExchangeRequest(ServiceDetail serviceDetail, Dictionary<string, string> parameters);

    /// <summary>
    /// Sends an authorization request to a service.
    /// </summary>
    /// <param name="serviceDetail">The service detail.</param>
    /// <param name="parameters">The authorization parameters (OAuth token and verifier code).</param>
    /// <returns></returns>
    Task<HttpResponseMessage> SendOAuth1Request(ServiceDetail serviceDetail, Dictionary<string, string> parameters);

    /// <summary>
    /// Sends a request to the service for a request token.
    /// </summary>
    /// <param name="url">The OAuth1 generated URL.</param>
    /// <returns></returns>
    Task<HttpResponseMessage> SendOAuth1RequestForRequestToken(string url);
}
