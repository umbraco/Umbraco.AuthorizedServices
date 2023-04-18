using System.Security.Cryptography;
using System.Text;
using Umbraco.AuthorizedServices.Models;

namespace Umbraco.AuthorizedServices.Services.Implement;

internal class AuthorizedServiceAuthorizationPayloadBuilder : IAuthorizedServiceAuthorizationPayloadBuilder
{
    public AuthorizedServiceAuthorizationPayload BuildPayload()
    {
        const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        var random = new Random();

        var generator = RandomNumberGenerator.Create();

        var state = new string(Enumerable.Repeat(chars, 7)
            .Select(s => s[random.Next(s.Length)]).ToArray());

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

        return new AuthorizedServiceAuthorizationPayload
        {
            State = state,
            CodeVerifier = codeVerifier,
            CodeChallenge = codeChallenge
        };
    }
}
