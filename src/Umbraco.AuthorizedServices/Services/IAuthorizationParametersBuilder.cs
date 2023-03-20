using Umbraco.AuthorizedServices.Configuration;

namespace Umbraco.AuthorizedServices.Services;

public interface IAuthorizationParametersBuilder
{
    Dictionary<string, string> BuildParameters(ServiceDetail serviceDetail, string authorizationCode, string redirectUri);
}
