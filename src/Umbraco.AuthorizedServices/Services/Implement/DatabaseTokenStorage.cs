using StackExchange.Profiling.Internal;
using Umbraco.AuthorizedServices.Models;
using Umbraco.AuthorizedServices.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Scoping;

namespace Umbraco.AuthorizedServices.Services.Implement;

internal sealed class DatabaseTokenStorage : ITokenStorage
{
    private readonly IScopeProvider _scopeProvider;

    private readonly ISecretEncryptor _encryptor;

    public DatabaseTokenStorage(IScopeProvider scopeProvider, ISecretEncryptor encryptor)
    {
        _scopeProvider = scopeProvider;
        _encryptor = encryptor;
    }

    public Token? GetToken(string serviceAlias)
    {
        using IScope scope = _scopeProvider.CreateScope();

        TokenDto entity = scope.Database.FirstOrDefault<TokenDto>("where serviceAlias = @0", serviceAlias);

        return entity != null
            ? new Token(
                _encryptor.Decrypt(entity.AccessToken),
                !string.IsNullOrEmpty(entity.RefreshToken)
                    ? _encryptor.Decrypt(entity.RefreshToken)
                    : string.Empty,
                entity.ExpiresOn)
            : null;
    }

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

    public void DeleteToken(string serviceAlias)
    {
        using IScope scope = _scopeProvider.CreateScope();

        TokenDto entity = scope.Database.Single<TokenDto>("where serviceAlias = @0", serviceAlias);

        scope.Database.Delete(entity);

        scope.Complete();
    }
}
