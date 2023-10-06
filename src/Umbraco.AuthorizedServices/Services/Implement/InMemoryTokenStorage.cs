using Umbraco.AuthorizedServices.Models;

namespace Umbraco.AuthorizedServices.Services.Implement;

/// <summary>
/// Implements <see cref="ITokenStorage"/> for token storage using in memory storage.
/// </summary>
internal sealed class InMemoryTokenStorage : ITokenStorage<Token>
{
    private static readonly Dictionary<string, Token> _tokens = new Dictionary<string, Token>();

    /// <inheritdoc/>
    public Token? GetToken(string serviceAlias)
    {
        if (_tokens.ContainsKey(serviceAlias))
        {
            return _tokens[serviceAlias];
        }

        return null;
    }

    /// <inheritdoc/>
    public void SaveToken(string serviceAlias, Token token)
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

    /// <inheritdoc/>
    public void DeleteToken(string serviceAlias) => _tokens.Remove(serviceAlias);
}
