using Microsoft.Extensions.Logging;
using Umbraco.AuthorizedServices.Models;
using Umbraco.AuthorizedServices.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Scoping;

namespace Umbraco.AuthorizedServices.Services.Implement;

/// <summary>
/// Implements <see cref="ITokenStorage{T}"/> for token storage using a database table.
/// </summary>
internal sealed class DatabaseOAuth1aTokenStorage : DatabaseAuthorizationParameterStorageBase, ITokenStorage<OAuth1aToken>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DatabaseTokenStorage"/> class.
    /// </summary>
    public DatabaseOAuth1aTokenStorage(IScopeProvider scopeProvider, ISecretEncryptor encryptor, ILogger<DatabaseTokenStorage> logger)
        : base(scopeProvider, encryptor, logger)
    {
    }

    /// <inheritdoc/>
    public OAuth1aToken? GetToken(string serviceAlias)
    {
        using IScope scope = ScopeProvider.CreateScope();

        OAuth1aTokenDto entity = scope.Database.FirstOrDefault<OAuth1aTokenDto>("where serviceAlias = @0", serviceAlias);
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

        return new OAuth1aToken(oauthToken, oauthTokenSecret);
    }

    private void RemoveCorruptToken(string serviceAlias, string tokenType)
    {
        DeleteToken(serviceAlias);
        Logger.LogWarning($"Could not decrypt the stored {tokenType} token for authorized service with alias '{serviceAlias}'. Token has been removed from storage.");
    }

    /// <inheritdoc/>
    public void SaveToken(string serviceAlias, OAuth1aToken token)
    {
        using IScope scope = ScopeProvider.CreateScope();

        OAuth1aTokenDto entity = scope.Database.SingleOrDefault<OAuth1aTokenDto>("where serviceAlias = @0", serviceAlias);

        bool insert = entity == null;
        entity ??= new OAuth1aTokenDto { ServiceAlias = serviceAlias };

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

        OAuth1aTokenDto entity = scope.Database.Single<OAuth1aTokenDto>("where serviceAlias = @0", serviceAlias);

        scope.Database.Delete(entity);

        scope.Complete();
    }
}
