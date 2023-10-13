using Microsoft.Extensions.Logging;
using Umbraco.AuthorizedServices.Models;
using Umbraco.AuthorizedServices.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Scoping;

namespace Umbraco.AuthorizedServices.Services.Implement;

/// <summary>
/// Implements <see cref="IOAuth1TokenStorage"/> for token storage using a database table.
/// </summary>
internal sealed class DatabaseOAuth1TokenStorage : DatabaseAuthorizationParameterStorageBase, IOAuth1TokenStorage
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DatabaseOAuth2TokenStorage"/> class.
    /// </summary>
    public DatabaseOAuth1TokenStorage(IScopeProvider scopeProvider, ISecretEncryptor encryptor, ILogger<DatabaseOAuth2TokenStorage> logger)
        : base(scopeProvider, encryptor, logger)
    {
    }

    /// <inheritdoc/>
    public OAuth1Token? GetToken(string serviceAlias)
    {
        using IScope scope = ScopeProvider.CreateScope();

        OAuth1TokenDto entity = scope.Database.FirstOrDefault<OAuth1TokenDto>("where serviceAlias = @0", serviceAlias);
        if (entity == null)
        {
            return null;
        }

        if (!Encryptor.TryDecrypt(entity.OAuthToken, out string oauthToken))
        {
            RemoveCorruptToken(serviceAlias, "access");
            return null;
        }

        if (!Encryptor.TryDecrypt(entity.OAuthTokenSecret, out string oauthTokenSecret))
        {
            RemoveCorruptToken(serviceAlias, "secret");
            return null;
        }

        return new OAuth1Token(oauthToken, oauthTokenSecret);
    }

    private void RemoveCorruptToken(string serviceAlias, string tokenType)
    {
        DeleteToken(serviceAlias);
        Logger.LogWarning($"Could not decrypt the stored {tokenType} token for authorized service with alias '{serviceAlias}'. Token has been removed from storage.");
    }

    /// <inheritdoc/>
    public void SaveToken(string serviceAlias, OAuth1Token token)
    {
        using IScope scope = ScopeProvider.CreateScope();

        OAuth1TokenDto entity = scope.Database.SingleOrDefault<OAuth1TokenDto>("where serviceAlias = @0", serviceAlias);

        bool insert = entity == null;
        entity ??= new OAuth1TokenDto { ServiceAlias = serviceAlias };

        entity.OAuthToken = Encryptor.Encrypt(token.OAuthToken);
        entity.OAuthTokenSecret = Encryptor.Encrypt(token.OAuthTokenSecret);

        if (insert)
        {
            scope.Database.Insert(entity);
        }
        else
        {
            scope.Database.Update(entity);
        }

        scope.Complete();
    }

    /// <inheritdoc/>
    public void DeleteToken(string serviceAlias)
    {
        using IScope scope = ScopeProvider.CreateScope();

        OAuth1TokenDto entity = scope.Database.Single<OAuth1TokenDto>("where serviceAlias = @0", serviceAlias);

        scope.Database.Delete(entity);

        scope.Complete();
    }
}
