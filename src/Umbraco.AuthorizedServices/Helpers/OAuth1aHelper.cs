using System.Security.Cryptography;
using System.Text;
using Umbraco.AuthorizedServices.Models;
using Umbraco.AuthorizedServices.Persistence.Dtos;

namespace Umbraco.AuthorizedServices.Helpers;

internal static class OAuth1aHelper
{
    public static string GetTimestamp() =>
        Convert.ToInt64((DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds).ToString();

    public static string GetNonce() =>
        Convert.ToBase64String(new ASCIIEncoding().GetBytes(DateTime.Now.Ticks.ToString()));

    public static string GetSignature(
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

    public static string GetAuthorizedSignature(
        string httpMethod,
        string url,
        string consumerSecret,
        OAuth1aToken oauth1aToken,
        Dictionary<string, string> authorizationParams)
    {
        string hashingKey = string.Format("{0}&{1}", consumerSecret, oauth1aToken.OAuthTokenSecret);

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

    public static bool TryParseOAuth1aResponse(this string? response, out string oauthToken, out string oauthTokenSecret)
    {
        oauthToken = string.Empty;
        oauthTokenSecret = string.Empty;

        if (response is null)
        {
            return false;
        }

        var responseParametersArray = response.Split('&');

        foreach (var parameter in responseParametersArray)
        {
            var kvpArray = parameter.Split('=');

            if (kvpArray[0] == Constants.OAuth1a.OAuthToken)
            {
                oauthToken = kvpArray[1];
            }

            if (kvpArray[0] == Constants.OAuth1a.OAuthTokenSecret)
            {
                oauthTokenSecret = kvpArray[1];
            }
        }

        return !string.IsNullOrWhiteSpace(oauthToken) || !string.IsNullOrWhiteSpace(oauthTokenSecret);
    }
}
