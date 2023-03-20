using Umbraco.AuthorizedServices.Configuration;

namespace Umbraco.AuthorizedServices.Services;

public interface IAuthorizationRequestSender
{
    Task<HttpResponseMessage> SendRequest(ServiceDetail serviceDetail, Dictionary<string, string> parameters);
}
