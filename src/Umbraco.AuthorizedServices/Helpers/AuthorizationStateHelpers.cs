namespace Umbraco.AuthorizedServices.Helpers;

internal class AuthorizationStateHelpers
{
    public static string GenerateStateString(int length = 7)
    {
        const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        var random = new Random();

        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }
}
