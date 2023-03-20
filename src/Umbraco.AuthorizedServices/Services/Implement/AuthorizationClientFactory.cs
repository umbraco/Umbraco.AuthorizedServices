using System.Net.Http.Headers;

namespace Umbraco.AuthorizedServices.Services.Implement;

internal sealed class AuthorizationClientFactory : IAuthorizationClientFactory
{
    private readonly IHttpClientFactory _httpClientFactory;

    public AuthorizationClientFactory(IHttpClientFactory httpClientFactory) => _httpClientFactory = httpClientFactory;

    public HttpClient CreateClient()
    {
        HttpClient httpClient = _httpClientFactory.CreateClient();
        httpClient.DefaultRequestHeaders.Accept.Clear();
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        return httpClient;
    }
}
