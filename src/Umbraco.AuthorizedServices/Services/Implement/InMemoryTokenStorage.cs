using Umbraco.AuthorizedServices.Models;

namespace Umbraco.AuthorizedServices.Services.Implement;

/// <summary>
/// Implements <see cref="IOAuth2TokenStorage"/> for token storage using in memory storage.
/// </summary>
internal sealed class InMemoryTokenStorage : IOAuth2TokenStorage
{
    private static readonly Dictionary<string, OAuth2Token> _tokens = new Dictionary<string, OAuth2Token>();

    /// <inheritdoc/>
    public OAuth2Token? GetToken(string serviceAlias)
    {
        if (_tokens.ContainsKey(serviceAlias))
        {
            return _tokens[serviceAlias];
        }

        return null;
    }

    /// <inheritdoc/>
    public void SaveToken(string serviceAlias, OAuth2Token token)
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
