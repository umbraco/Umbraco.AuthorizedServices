using Newtonsoft.Json.Linq;
using Umbraco.AuthorizedServices.Configuration;
using Umbraco.AuthorizedServices.Helpers;
using Umbraco.AuthorizedServices.Models;

namespace Umbraco.AuthorizedServices.Services.Implement;

internal sealed class TokenFactory : ITokenFactory
{
    private readonly IDateTimeProvider _dateTimeProvider;

    public TokenFactory(IDateTimeProvider dateTimeProvider) => _dateTimeProvider = dateTimeProvider;

    public OAuth2Token CreateFromOAuth2ResponseContent(string responseContent, ServiceDetail serviceDetail)
    {
        var tokenResponse = JObject.Parse(responseContent);

        var accessToken = tokenResponse[serviceDetail.AccessTokenResponseKey]?.ToString();
        if (string.IsNullOrEmpty(accessToken))
        {
            throw new InvalidOperationException($"Could not retrieve access token using key '{serviceDetail.AccessTokenResponseKey}' from the token response from '{serviceDetail.Alias}'");
        }

        var refreshToken = tokenResponse[serviceDetail.RefreshTokenResponseKey]?.ToString();

        DateTime? expiresOn = null;
        var expiresInValue = tokenResponse[serviceDetail.ExpiresInResponseKey]?.ToString();
        if (!string.IsNullOrEmpty(expiresInValue) && !expiresInValue.Equals("0"))
        {
            var expiresInSeconds = int.Parse(expiresInValue);
            expiresOn = _dateTimeProvider.UtcNow().AddSeconds(expiresInSeconds);
        }

        return new OAuth2Token(accessToken, refreshToken, expiresOn);
    }

    public OAuth1Token CreateFromOAuth1ResponseContent(string responseContent)
    {
        if (!responseContent.TryParseOAuth1Response(out var oauthToken, out var oauthTokenSecret))
        {
            throw new InvalidOperationException($"Invalid response content: {responseContent}");
        }

        return new OAuth1Token(oauthToken, oauthTokenSecret);
    }
}
