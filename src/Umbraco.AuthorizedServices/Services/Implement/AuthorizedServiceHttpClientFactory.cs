using Umbraco.AuthorizedServices.Configuration;

namespace Umbraco.AuthorizedServices.Services.Implement;

internal sealed class AuthorizedServiceHttpClientFactory : IAuthorizedServiceHttpClientFactory
{
    private readonly IHttpClientFactory _httpClientFactory;

    public AuthorizedServiceHttpClientFactory(IHttpClientFactory httpClientFactory) => _httpClientFactory = httpClientFactory;

    public HttpClient CreateClient(ServiceDetail serviceDetail, string path, HttpMethod httpMethod) => _httpClientFactory.CreateClient();
}
