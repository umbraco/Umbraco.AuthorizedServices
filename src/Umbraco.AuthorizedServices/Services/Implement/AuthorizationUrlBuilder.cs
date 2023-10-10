using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;
using Umbraco.AuthorizedServices.Configuration;
using Umbraco.AuthorizedServices.Extensions;
using Umbraco.AuthorizedServices.Helpers;

namespace Umbraco.AuthorizedServices.Services.Implement;

internal sealed class AuthorizationUrlBuilder : IAuthorizationUrlBuilder
{
    public string BuildOAuth1RequestTokenUrl(ServiceDetail serviceDetail,HttpContext? httpContext, HttpMethod httpMethod)
    {
        string nonce = OAuth1Helper.GetNonce();
        string timestamp = OAuth1Helper.GetTimestamp();
        var authorizationParams =
            new Dictionary<string, string>
            {
                { "oauth_consumer_key", serviceDetail.ClientId },
                { "oauth_nonce", nonce },
                { "oauth_signature_method", "HMAC-SHA1" },
                { "oauth_timestamp", timestamp },
                { "oauth_version", "1.0" }
            };

        var url = new StringBuilder();
        url.Append(serviceDetail.IdentityHost);
        url.Append(serviceDetail.RequestAuthorizationPath);

        if (serviceDetail.AuthorizationUrlRequiresRedirectUrl)
        {
            var redirectUri = httpContext?.GetOAuth1AuthorizedServiceRedirectUri();

            if (redirectUri is not null)
            {
                authorizationParams.Add("oauth_callback", redirectUri);
                url.Append("?oauth_callback=").Append(Uri.EscapeDataString(redirectUri));
            }
        }

        url
            .Append($"{(serviceDetail.AuthorizationUrlRequiresRedirectUrl ? "&" : "?")}oauth_consumer_key=")
            .Append(serviceDetail.ClientId);
        url.Append("&oauth_nonce=").Append(nonce);

        var signature = GetSignature(
            httpMethod.Method.ToUpper(),
            $"{serviceDetail.IdentityHost}{serviceDetail.RequestAuthorizationPath}",
            serviceDetail.ClientSecret,
            authorizationParams);
        url.Append("&oauth_signature=").Append(Uri.EscapeDataString(signature));
        url.Append("&oauth_signature_method=HMAC-SHA1");
        url.Append("&oauth_timestamp=").Append(timestamp);
        url.Append("&oauth_version=1.0");

        return url.ToString();
    }

    public string BuildOAuth2AuthorizationUrl(ServiceDetail serviceDetail, HttpContext httpContext, string state, string codeChallenge)
    {
        var url = new StringBuilder();
        url.Append(serviceDetail.IdentityHost);
        url.Append(serviceDetail.RequestIdentityPath);
        url.Append("?response_type=code");
        url.Append("&client_id=").Append(serviceDetail.ClientId);
        url.Append("&response_mode=query");

        if (serviceDetail.AuthorizationUrlRequiresRedirectUrl)
        {
            url.Append("&redirect_uri=").Append(httpContext.GetOAuth2AuthorizedServiceRedirectUri());
        }

        url.Append("&scope=").Append(serviceDetail.Scopes);

        url.Append("&state=").Append(serviceDetail.Alias + Constants.Separator + state);

        if (serviceDetail.UseProofKeyForCodeExchange)
        {
            url.Append("&code_challenge=").Append(codeChallenge);
            url.Append("&code_challenge_method=S256");
        }

        return url.ToString();
    }

    private string GetSignature(
        string httpMethod,
        string url,
        string consumerSecret,
        Dictionary<string, string> authorizationParams)
    {
        string hashingKey = string.Format("{0}&", consumerSecret);

        using var hasher = new HMACSHA1(new ASCIIEncoding().GetBytes(hashingKey));

        string authorizationParamsStr = string.Join(
            "&",
            authorizationParams
                .Select(kvp => string.Format("{0}={1}", Uri.EscapeDataString(kvp.Key), Uri.EscapeDataString(kvp.Value)))
                .OrderBy(p => p));

        // signature format: HTTP method (uppercase) + & + request URL + & + authorization parameters
        string signature = string.Format(
            "{0}&{1}&{2}",
            httpMethod,
            Uri.EscapeDataString(url),
            Uri.EscapeDataString(authorizationParamsStr));

        return Convert.ToBase64String(hasher.ComputeHash(new ASCIIEncoding().GetBytes(signature)));
    }
}
