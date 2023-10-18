using Microsoft.Extensions.Logging;
using Umbraco.AuthorizedServices.Models;
using Umbraco.AuthorizedServices.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Scoping;

namespace Umbraco.AuthorizedServices.Services.Implement;

/// <summary>
/// Implements <see cref="IOAuth2TokenStorage"/> for token storage using a database table.
/// </summary>
internal sealed class DatabaseOAuth2TokenStorage : DatabaseAuthorizationParameterStorageBase, IOAuth2TokenStorage
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DatabaseOAuth2TokenStorage"/> class.
    /// </summary>
    public DatabaseOAuth2TokenStorage(IScopeProvider scopeProvider, ISecretEncryptor encryptor, ILogger<DatabaseOAuth2TokenStorage> logger)
        : base(scopeProvider, encryptor, logger)
    {
    }

    /// <inheritdoc/>
    public OAuth2Token? GetToken(string serviceAlias)
    {
        using IScope scope = ScopeProvider.CreateScope();

        OAuth2TokenDto entity = scope.Database.FirstOrDefault<OAuth2TokenDto>("where serviceAlias = @0", serviceAlias);
        if (entity == null)
        {
            return null;
        }

        if (!Encryptor.TryDecrypt(entity.AccessToken, out string accessToken))
        {
            RemoveCorruptToken(serviceAlias, "access");
            return null;
        }

        var refreshToken = string.Empty;
        if (!string.IsNullOrEmpty(entity.RefreshToken))
        {
            if (!Encryptor.TryDecrypt(entity.RefreshToken, out refreshToken))
            {
                RemoveCorruptToken(serviceAlias, "refresh");
                return null;
            }
        }

        return new OAuth2Token(accessToken, refreshToken, entity.ExpiresOn);
    }

    private void RemoveCorruptToken(string serviceAlias, string tokenType)
    {
        DeleteToken(serviceAlias);
        Logger.LogWarning($"Could not decrypt the stored {tokenType} token for authorized service with alias '{serviceAlias}'. Token has been removed from storage.");
    }

    /// <inheritdoc/>
    public void SaveToken(string serviceAlias, OAuth2Token token)
    {
        using IScope scope = ScopeProvider.CreateScope();

        OAuth2TokenDto entity = scope.Database.SingleOrDefault<OAuth2TokenDto>("where serviceAlias = @0", serviceAlias);

        bool insert = entity == null;
        entity ??= new OAuth2TokenDto { ServiceAlias = serviceAlias };

        entity.AccessToken = Encryptor.Encrypt(token.AccessToken);
        entity.RefreshToken = !string.IsNullOrEmpty(token.RefreshToken)
            ? Encryptor.Encrypt(token.RefreshToken)
            : string.Empty;
        entity.ExpiresOn = token.ExpiresOn;

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

        OAuth2TokenDto entity = scope.Database.Single<OAuth2TokenDto>("where serviceAlias = @0", serviceAlias);

        scope.Database.Delete(entity);

        scope.Complete();
    }
}
