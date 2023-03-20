using System.Text;
using Umbraco.AuthorizedServices.Configuration;

namespace Umbraco.AuthorizedServices.Services.Implement;

internal sealed class AuthorizationRequestSender : IAuthorizationRequestSender
{
    private readonly IAuthorizationClientFactory _authorizationClientFactory;

    public AuthorizationRequestSender(IAuthorizationClientFactory authorizationClientFactory) => _authorizationClientFactory = authorizationClientFactory;

    public async Task<HttpResponseMessage> SendRequest(ServiceDetail serviceDetail, Dictionary<string, string> parameters)
    {
        HttpClient httpClient = _authorizationClientFactory.CreateClient();

        var url = serviceDetail.GetTokenHost() + serviceDetail.RequestTokenPath;

        HttpContent? content = null;
        switch (serviceDetail.RequestTokenFormat)
        {
            case TokenRequestContentFormat.Querystring:
                url += BuildAuthorizationQuerystring(parameters);
                break;
            case TokenRequestContentFormat.FormUrlEncoded:
                content = new FormUrlEncodedContent(parameters);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(serviceDetail.RequestTokenFormat));
        }

        return await httpClient.PostAsync(url, content);
    }

    private static string BuildAuthorizationQuerystring(Dictionary<string, string> parameters)
    {
        var qs = new StringBuilder();
        var sep = "?";
        foreach (KeyValuePair<string, string> parameter in parameters)
        {
            qs.Append(sep).Append(parameter.Key).Append("=").Append(parameter.Value);
            sep = "&";
        }

        return qs.ToString();
    }
}
