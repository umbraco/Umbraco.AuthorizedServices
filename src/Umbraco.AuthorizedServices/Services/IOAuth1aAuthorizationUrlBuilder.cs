using Umbraco.AuthorizedServices.Configuration;

namespace Umbraco.AuthorizedServices.Services;

public interface IOAuth1aAuthorizationUrlBuilder
{
    string BuildAuthorizationUrl(ServiceDetail serviceDetail, string path);

    string BuildRequestTokenUrl(ServiceDetail serviceDetail, HttpMethod httpMethod);
}
