using Umbraco.AuthorizedServices.Configuration;

namespace Umbraco.AuthorizedServices.Services;

/// <summary>
/// Defines operations on creation of HTTP clients for authorized services.
/// </summary>
public interface IAuthorizedServiceHttpClientFactory
{
    HttpClient CreateClient(ServiceDetail serviceDetail, string path, HttpMethod httpMethod);
}
