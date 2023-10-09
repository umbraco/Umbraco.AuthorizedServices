using System.Text;
using Microsoft.AspNetCore.Http;
using Umbraco.AuthorizedServices.Configuration;
using Umbraco.AuthorizedServices.Extensions;
using Umbraco.AuthorizedServices.Helpers;

namespace Umbraco.AuthorizedServices.Services.Implement;

internal class OAuth1aAuthorizationUrlBuilder : IOAuth1aAuthorizationUrlBuilder
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public OAuth1aAuthorizationUrlBuilder(IHttpContextAccessor httpContextAccessor) =>
        _httpContextAccessor = httpContextAccessor;

    public string BuildAuthorizationUrl(ServiceDetail serviceDetail, string path)
    {
        var url = new StringBuilder();
        url.Append(serviceDetail.IdentityHost);
        url.Append(serviceDetail.RequestIdentityPath);
        url.Append("?").Append(path);

        return url.ToString();
    }

    public string BuildRequestTokenUrl(ServiceDetail serviceDetail, HttpMethod httpMethod)
    {
        string nonce = OAuth1aHelper.GetNonce();
        string timestamp = OAuth1aHelper.GetTimestamp();
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
            var redirectUri = _httpContextAccessor.HttpContext?.GetOAuth1AuthorizedServiceRedirectUri();

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

        var signature = OAuth1aHelper.GetSignature(
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
}
