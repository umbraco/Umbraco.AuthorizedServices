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

    public Dictionary<string, string> BuildParametesForOAuth2AccessTokenExchange(ServiceDetail serviceDetail, string accessToken) =>
        new Dictionary<string, string>
        {
            {
                "grant_type", serviceDetail.ExchangeTokenProvision is not null ? serviceDetail.ExchangeTokenProvision.RefreshTokenGrantType : string.Empty
            },
            { "access_token", accessToken }
        };
}
