using Umbraco.AuthorizedServices.Configuration;

namespace Umbraco.AuthorizedServices.Services.Implement;

internal sealed class AuthorizationParametersBuilder : IAuthorizationParametersBuilder
{
    public Dictionary<string, string> BuildParameters(ServiceDetail serviceDetail, string authorizationCode, string redirectUri) =>
        new Dictionary<string, string>
            {
                    { "grant_type", "authorization_code" },
                    { "client_id", serviceDetail.ClientId },
                    { "client_secret", serviceDetail.ClientSecret },
                    { "code", authorizationCode },
                    { "redirect_uri", redirectUri }
            };
}
