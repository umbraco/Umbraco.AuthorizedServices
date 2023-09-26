using System.Net.Http.Headers;
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

        if ((serviceDetail.AuthenticationMethod == AuthenticationMethod.OAuth2ClientCredentials
                && serviceDetail.ClientCredentialsProvision == ClientCredentialsProvision.AuthHeader)
            || serviceDetail.AuthorizationRequestRequiresAuthorizationHeaderWithBasicToken)
        {
            BuildBasicTokenHeader(httpClient, serviceDetail);
        }

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

    public async Task<HttpResponseMessage> SendExchangeRequest(ServiceDetail serviceDetail, Dictionary<string, string> parameters)
    {
        HttpClient httpClient = _authorizationClientFactory.CreateClient();

        var url = serviceDetail.ExchangeTokenProvision is not null
            ? string.Format(
                "{0}{1}",
                string.IsNullOrWhiteSpace(serviceDetail.ExchangeTokenProvision.TokenHost)
                    ? serviceDetail.GetTokenHost()
                    : serviceDetail.ExchangeTokenProvision.TokenHost,
                string.IsNullOrWhiteSpace(serviceDetail.ExchangeTokenProvision.RequestTokenPath)
                    ? serviceDetail.RequestTokenPath
                    : serviceDetail.ExchangeTokenProvision.RequestTokenPath)
            : serviceDetail.GetTokenHost() + serviceDetail.RequestTokenPath;

        if (serviceDetail.ExchangeTokenProvision is null)
        {
            throw new ArgumentNullException(nameof(serviceDetail.ExchangeTokenProvision));
        }

        url += BuildAuthorizationQuerystring(parameters);

        return await httpClient.GetAsync(url);
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

    private static void BuildBasicTokenHeader(HttpClient httpClient, ServiceDetail serviceDetail)
    {
        var authenticationString = $"{serviceDetail.ClientId}:{serviceDetail.ClientSecret}";
        var base64String = Convert.ToBase64String(Encoding.UTF8.GetBytes(authenticationString));

        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", base64String);
    }
}
