namespace Umbraco.AuthorizedServices.Services.Implement;

internal sealed class TokenCache : ITokenCache
{
    private static readonly Dictionary<string, string> _tokens = new Dictionary<string, string>();

    public void Delete(string serviceAlias) => _tokens.Remove(serviceAlias);

    public string? Get(string serviceAlias)
    {
        if (_tokens.ContainsKey(serviceAlias))
        {
            return _tokens[serviceAlias];
        }

        return null;
    }

    public string? GetByValue(string token)
    {
        if (_tokens.ContainsValue(token))
        {
            return _tokens.First(p => p.Value == token).Key;
        }

        return null;
    }

    public void Save(string serviceAlias, string token)
    {
        if (_tokens.ContainsKey(serviceAlias))
        {
            _tokens[serviceAlias] = token;
        }
        else
        {
            _tokens.Add(serviceAlias, token);
        }
    }
}
