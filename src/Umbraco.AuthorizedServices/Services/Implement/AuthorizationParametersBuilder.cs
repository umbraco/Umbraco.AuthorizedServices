using Umbraco.AuthorizedServices.Configuration;

namespace Umbraco.AuthorizedServices.Services.Implement;

internal sealed class AuthorizationParametersBuilder : IAuthorizationParametersBuilder
{
    public Dictionary<string, string> BuildParametersForOAuth2AuthorizationCode(ServiceDetail serviceDetail, string authorizationCode, string redirectUri, string codeVerifier)
    {
        var parametersDictionary = new Dictionary<string, string>
            {
                { "grant_type", "authorization_code" },
                { "client_id", serviceDetail.ClientId },
                { "client_secret", serviceDetail.ClientSecret },
                { "code", authorizationCode },
                { "redirect_uri", redirectUri }
            };

        if (serviceDetail.UseProofKeyForCodeExchange)
        {
            parametersDictionary.Add("code_verifier", codeVerifier);
        }

        return parametersDictionary;
    }

    public Dictionary<string, string> BuildParametersForOAuth2ClientCredentials(ServiceDetail serviceDetail)
    {
        var parametersDictionary = new Dictionary<string, string>()
        {
            { "grant_type", "client_credentials" }
        };

        if (serviceDetail.ClientCredentialsProvision == ClientCredentialsProvision.RequestBody)
        {
            parametersDictionary.Add("client_id", serviceDetail.ClientId);
            parametersDictionary.Add("client_secret", serviceDetail.ClientSecret);
        }

        if (serviceDetail.IncludeScopesInAuthorizationRequest)
        {
            parametersDictionary.Add("scope", serviceDetail.Scopes);
        }

        return parametersDictionary;
    }

    public Dictionary<string, string> BuildParametesForOAuth2AccessTokenExchange(ServiceDetail serviceDetail, string? accessToken)
    {
        var parametersDictionary = new Dictionary<string, string>()
        {
            { "grant_type", serviceDetail.ExchangeTokenProvision is not null ? serviceDetail.ExchangeTokenProvision.TokenGrantType : string.Empty },
            { "client_secret", serviceDetail.ClientSecret }
        };

        if (accessToken is not null)
        {
            parametersDictionary.Add("access_token", accessToken);
        }

        return parametersDictionary;
    }
}
