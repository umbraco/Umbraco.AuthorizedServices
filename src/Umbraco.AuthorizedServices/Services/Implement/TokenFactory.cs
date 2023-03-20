using Newtonsoft.Json.Linq;
using Umbraco.AuthorizedServices.Configuration;
using Umbraco.AuthorizedServices.Models;

namespace Umbraco.AuthorizedServices.Services.Implement;

internal sealed class TokenFactory : ITokenFactory
{
    public Token CreateFromResponseContent(string responseContent, string serviceAlias, ServiceDetail serviceDetail)
    {
        var tokenResponse = JObject.Parse(responseContent);

        var accessToken = tokenResponse[serviceDetail.AccessTokenResponseKey]?.ToString();
        if (string.IsNullOrEmpty(accessToken))
        {
            throw new InvalidOperationException($"Could not retrieve access token using key '{serviceDetail.AccessTokenResponseKey}' from the token response from '{serviceAlias}'");
        }

        var refreshToken = tokenResponse[serviceDetail.RefreshTokenResponseKey]?.ToString();

        DateTime? expiresOn = null;
        var expiresInValue = tokenResponse[serviceDetail.ExpiresInResponseKey]?.ToString();
        if (!string.IsNullOrEmpty(expiresInValue))
        {
            var expiresInSeconds = int.Parse(expiresInValue);
            expiresOn = DateTime.Now.AddSeconds(expiresInSeconds);
        }

        return new Token(accessToken, refreshToken, expiresOn);
    }
}
