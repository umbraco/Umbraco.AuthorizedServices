using Microsoft.Extensions.Logging;
using Umbraco.AuthorizedServices.Models;
using Umbraco.AuthorizedServices.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Scoping;

namespace Umbraco.AuthorizedServices.Services.Implement;

/// <summary>
/// Implements <see cref="ITokenStorage"/> for token storage using a database table.
/// </summary>
internal sealed class DatabaseTokenStorage : ITokenStorage
{
    private readonly IScopeProvider _scopeProvider;
    private readonly ISecretEncryptor _encryptor;
    private readonly ILogger<DatabaseTokenStorage> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="DatabaseTokenStorage"/> class.
    /// </summary>
    public DatabaseTokenStorage(IScopeProvider scopeProvider, ISecretEncryptor encryptor, ILogger<DatabaseTokenStorage> logger)
    {
        _scopeProvider = scopeProvider;
        _encryptor = encryptor;
        _logger = logger;
    }

    /// <inheritdoc/>
    public Token? GetToken(string serviceAlias)
    {
        using IScope scope = _scopeProvider.CreateScope();

        TokenDto entity = scope.Database.FirstOrDefault<TokenDto>("where serviceAlias = @0", serviceAlias);
        if (entity == null)
        {
            return null;
        }

        if (!_encryptor.TryDecrypt(entity.AccessToken, out string accessToken))
        {
            RemoveCorruptToken(serviceAlias, "access");
            return null;
        }

        var refreshToken = string.Empty;
        if (!string.IsNullOrEmpty(entity.RefreshToken))
        {
            if (!_encryptor.TryDecrypt(entity.RefreshToken, out refreshToken))
            {
                RemoveCorruptToken(serviceAlias, "refresh");
                return null;
            }
        }

        return new Token(accessToken, refreshToken, entity.ExpiresOn);
    }

    private void RemoveCorruptToken(string serviceAlias, string tokenType)
    {
        DeleteToken(serviceAlias);
        _logger.LogWarning($"Could not decrypt the stored {tokenType} token for authorized service with alias '{serviceAlias}'. Token has been removed from storage.");
    }

    /// <inheritdoc/>
    public void SaveToken(string serviceAlias, Token token)
    {
        using IScope scope = _scopeProvider.CreateScope();

        TokenDto entity = scope.Database.SingleOrDefault<TokenDto>("where serviceAlias = @0", serviceAlias);

        bool insert = entity == null;
        entity ??= new TokenDto { ServiceAlias = serviceAlias };

        entity.AccessToken = _encryptor.Encrypt(token.AccessToken);
        entity.RefreshToken = !string.IsNullOrEmpty(token.RefreshToken)
            ? _encryptor.Encrypt(token.RefreshToken)
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
        using IScope scope = _scopeProvider.CreateScope();

        TokenDto entity = scope.Database.Single<TokenDto>("where serviceAlias = @0", serviceAlias);

        scope.Database.Delete(entity);

        scope.Complete();
    }
}
