using Umbraco.AuthorizedServices.Configuration;

namespace Umbraco.AuthorizedServices.Services.Implement;

internal sealed class AuthorizationParametersBuilder : IAuthorizationParametersBuilder
{
    public Dictionary<string, string> BuildParameters(ServiceDetail serviceDetail, string authorizationCode, string redirectUri, string codeVerifier)
    {
        var parametersDictionary = new Dictionary<string, string>();

        if (serviceDetail.IncludeScopesInAuthorizationRequest)
        {
            parametersDictionary.Add("scope", serviceDetail.Scopes);
        }

        if (serviceDetail.AuthenticationMethod == AuthenticationMethod.OAuth2ClientCredentials)
        {
            parametersDictionary.Add("grant_type", "client_credentials");

            if (serviceDetail.ClientCredentialsProvision == ClientCredentialsProvision.RequestBody)
            {
                parametersDictionary.Add("client_id", serviceDetail.ClientId);
                parametersDictionary.Add("client_secret", serviceDetail.ClientSecret);
            }

            return parametersDictionary;
        }

        parametersDictionary = new Dictionary<string, string>
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
}
