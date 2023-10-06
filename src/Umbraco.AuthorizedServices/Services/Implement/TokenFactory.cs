using Newtonsoft.Json.Linq;
using Umbraco.AuthorizedServices.Configuration;
using Umbraco.AuthorizedServices.Helpers;
using Umbraco.AuthorizedServices.Models;

namespace Umbraco.AuthorizedServices.Services.Implement;

internal sealed class TokenFactory : ITokenFactory
{
    private readonly IDateTimeProvider _dateTimeProvider;

    public TokenFactory(IDateTimeProvider dateTimeProvider) => _dateTimeProvider = dateTimeProvider;

    public Token CreateFromResponseContent(string responseContent, ServiceDetail serviceDetail)
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

        return new Token(accessToken, refreshToken, expiresOn);
    }

    public OAuth1aToken CreateFromOAuth1aResponseContent(string responseContent, ServiceDetail serviceDetail)
    {
        if (!responseContent.TryParseOAuth1aResponse(out var oauthToken, out var oauthTokenSecret))
        {
            throw new InvalidOperationException($"Invalid response content: {responseContent}");
        }

        return new OAuth1aToken(oauthToken, oauthTokenSecret);
    }
}
