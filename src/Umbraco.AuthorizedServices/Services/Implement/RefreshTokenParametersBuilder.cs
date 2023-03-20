using Umbraco.AuthorizedServices.Configuration;

namespace Umbraco.AuthorizedServices.Services.Implement;

internal sealed class RefreshTokenParametersBuilder : IRefreshTokenParametersBuilder
{
    public Dictionary<string, string> BuildParameters(ServiceDetail serviceDetail, string refreshToken) =>
        new Dictionary<string, string>
            {
                    { "grant_type", "refresh_token" },
                    { "client_id", serviceDetail.ClientId },
                    { "client_secret", serviceDetail.ClientSecret },
                    { "refresh_token", refreshToken }
            };
}
