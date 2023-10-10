using System.Security.Cryptography;
using System.Text;
using Umbraco.AuthorizedServices.Models;

namespace Umbraco.AuthorizedServices.Helpers;

internal static class OAuth1Helper
{
    public static string GetTimestamp() =>
        Convert.ToInt64((DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds).ToString();

    public static string GetNonce() =>
        Convert.ToBase64String(new ASCIIEncoding().GetBytes(DateTime.Now.Ticks.ToString()));

    public static bool TryParseOAuth1Response(this string? response, out string oauthToken, out string oauthTokenSecret)
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

            if (kvpArray[0] == Constants.OAuth1.OAuthToken)
            {
                oauthToken = kvpArray[1];
            }

            if (kvpArray[0] == Constants.OAuth1.OAuthTokenSecret)
            {
                oauthTokenSecret = kvpArray[1];
            }
        }

        return !string.IsNullOrWhiteSpace(oauthToken) || !string.IsNullOrWhiteSpace(oauthTokenSecret);
    }
}
