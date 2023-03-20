using Umbraco.AuthorizedServices.Configuration;

namespace Umbraco.AuthorizedServices.Services;

public interface IRefreshTokenParametersBuilder
{
    Dictionary<string, string> BuildParameters(ServiceDetail serviceDetail, string refreshToken);
}
