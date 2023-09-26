using Umbraco.AuthorizedServices.Configuration;

namespace Umbraco.AuthorizedServices.Services.Implement;

internal class ExchangeTokenParametersBuilder : IExchangeTokenParametersBuilder
{
    public Dictionary<string, string> BuildParameters(ServiceDetail serviceDetail, string accessToken) =>
        new Dictionary<string, string>()
        {
            { "grant_type", serviceDetail.ExchangeTokenProvision is not null ? serviceDetail.ExchangeTokenProvision.TokenGrantType : string.Empty },
            { "client_secret", serviceDetail.ClientSecret },
            { "access_token", accessToken }
        };
}
