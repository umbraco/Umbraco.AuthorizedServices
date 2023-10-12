using Umbraco.AuthorizedServices.Configuration;
using Umbraco.AuthorizedServices.Helpers;

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

    public Dictionary<string, string> BuildParametersForOAuth1(ServiceDetail serviceDetail, string oauthToken, string oauthVerifier, string oauthTokenSecret)
    {
        string nonce = OAuth1Helper.GetNonce();
        string timestamp = OAuth1Helper.GetTimestamp();

        var parametersDictionary = new Dictionary<string, string>();
        if (serviceDetail.UseRequestTokenWithExtendedParametersList)
        {
            parametersDictionary.Add(Constants.OAuth1.OAuthConsumerKey, serviceDetail.ClientId);
            parametersDictionary.Add(Constants.OAuth1.OAuthNonce, nonce);
            parametersDictionary.Add(Constants.OAuth1.OAuthSignatureMethod, "HMAC-SHA1");
            parametersDictionary.Add(Constants.OAuth1.OAuthTimestamp, timestamp);
            parametersDictionary.Add(Constants.OAuth1.OAuthToken, oauthToken);
            parametersDictionary.Add(Constants.OAuth1.OAuthVerifier, oauthVerifier);
            parametersDictionary.Add(Constants.OAuth1.OAuthVersion, "1.0");

            var signature = OAuth1Helper.GetSignature(
                serviceDetail.RequestTokenMethod.Method,
                $"{serviceDetail.IdentityHost}{serviceDetail.RequestTokenPath}",
                serviceDetail.ClientSecret,
                oauthTokenSecret,
                parametersDictionary);

            parametersDictionary.Add(Constants.OAuth1.OAuthSignature, Uri.EscapeDataString(signature));
        }
        else
        {
            parametersDictionary.Add(Constants.OAuth1.OAuthToken, oauthToken);
            parametersDictionary.Add(Constants.OAuth1.OAuthVerifier, oauthVerifier);
        }

        return parametersDictionary;
    }
}
