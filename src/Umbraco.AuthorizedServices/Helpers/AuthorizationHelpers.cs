using System.Security.Cryptography;
using System.Text;
using Umbraco.AuthorizedServices.Models;

namespace Umbraco.AuthorizedServices.Helpers;

internal class AuthorizationHelpers
{
    public static AuthorizedServiceAuthorizationPayload GeneratePayload()
    {
        KeyValuePair<string, string> keys = GenerateKeys();
        return new AuthorizedServiceAuthorizationPayload
        {
            State = GenerateStateString(),
            CodeVerifier = keys.Key,
            CodeChallenge = keys.Value
        };
    }

    private static string GenerateStateString(int length = 7)
    {
        const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        var random = new Random();

        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }

    /// <summary>
    /// Generates a code verifier and its hash for PKCE OAuth flows.
    /// </summary>
    /// <returns>Code Verifier, Code Challenge pair</returns>
    private static KeyValuePair<string, string> GenerateKeys()
    {
        var random = new Random();

        var generator = RandomNumberGenerator.Create();

        var bytes = new byte[32];

        generator.GetBytes(bytes);

        var codeVerifier = Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');

        using var sh256 = SHA256.Create();

        var challengeBytes = sh256.ComputeHash(Encoding.UTF8.GetBytes(codeVerifier));

        var codeChallenge = Convert.ToBase64String(challengeBytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');

        return new KeyValuePair<string, string>(codeVerifier, codeChallenge);
    }
}
