using Microsoft.Extensions.Logging;
using NPoco;
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
    public async Task<OAuth1Token?> GetTokenAsync(string serviceAlias)
    {
        using IScope scope = ScopeProvider.CreateScope();

        OAuth1TokenDto entity = await scope.Database.FirstOrDefaultAsync<OAuth1TokenDto>("where serviceAlias = @0", serviceAlias);
        if (entity == null)
        {
            return null;
        }

        if (!Encryptor.TryDecrypt(entity.OAuthToken, out string oauthToken))
        {
            await RemoveCorruptToken(serviceAlias, "access");
            return null;
        }

        if (!Encryptor.TryDecrypt(entity.OAuthTokenSecret, out string oauthTokenSecret))
        {
            await RemoveCorruptToken(serviceAlias, "secret");
            return null;
        }

        return new OAuth1Token(oauthToken, oauthTokenSecret);
    }

    private async Task RemoveCorruptToken(string serviceAlias, string tokenType)
    {
        await DeleteTokenAsync(serviceAlias);
        Logger.LogWarning($"Could not decrypt the stored {tokenType} token for authorized service with alias '{serviceAlias}'. Token has been removed from storage.");
    }

    /// <inheritdoc/>
    public async Task SaveTokenAsync(string serviceAlias, OAuth1Token token)
    {
        using IScope scope = ScopeProvider.CreateScope();

        OAuth1TokenDto? entity = await scope.Database.SingleOrDefaultAsync<OAuth1TokenDto>("where serviceAlias = @0", serviceAlias);

        bool insert = entity == null;
        entity ??= new OAuth1TokenDto { ServiceAlias = serviceAlias };

        entity.OAuthToken = Encryptor.Encrypt(token.OAuthToken);
        entity.OAuthTokenSecret = Encryptor.Encrypt(token.OAuthTokenSecret);

        if (insert)
        {
            await scope.Database.InsertAsync(entity);
        }
        else
        {
            await scope.Database.UpdateAsync(entity);
        }

        scope.Complete();
    }

    /// <inheritdoc/>
    public async Task DeleteTokenAsync(string serviceAlias)
    {
        using IScope scope = ScopeProvider.CreateScope();

        OAuth1TokenDto? entity = await scope.Database.SingleOrDefaultAsync<OAuth1TokenDto>("where serviceAlias = @0", serviceAlias);
        if (entity is not null)
        {
            scope.Database.Delete(entity);
        }

        scope.Complete();
    }
}
