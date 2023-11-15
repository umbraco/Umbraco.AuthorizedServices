using Umbraco.AuthorizedServices.Models;

namespace Umbraco.AuthorizedServices.Services.Implement;

/// <summary>
/// Implements <see cref="IOAuth2TokenStorage"/> for token storage using in memory storage.
/// </summary>
internal sealed class InMemoryTokenStorage : IOAuth2TokenStorage
{
    private static readonly Dictionary<string, OAuth2Token> _tokens = new Dictionary<string, OAuth2Token>();

    /// <inheritdoc/>
    public Task<OAuth2Token?> GetTokenAsync(string serviceAlias)
    {
        if (_tokens.ContainsKey(serviceAlias))
        {
            return Task.FromResult((OAuth2Token?)_tokens[serviceAlias]);
        }

        return Task.FromResult((OAuth2Token?)null);
    }

    /// <inheritdoc/>
    public Task SaveTokenAsync(string serviceAlias, OAuth2Token token)
    {
        if (_tokens.ContainsKey(serviceAlias))
        {
            _tokens[serviceAlias] = token;
        }
        else
        {
            _tokens.Add(serviceAlias, token);
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task DeleteTokenAsync(string serviceAlias)
    {
        _tokens.Remove(serviceAlias);
        return Task.CompletedTask;
    }
}
