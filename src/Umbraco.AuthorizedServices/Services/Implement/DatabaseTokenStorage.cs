using Umbraco.AuthorizedServices.Models;
using Umbraco.AuthorizedServices.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Scoping;

namespace Umbraco.AuthorizedServices.Services.Implement;

internal sealed class DatabaseTokenStorage : ITokenStorage
{
    private readonly IScopeProvider _scopeProvider;

    public DatabaseTokenStorage(IScopeProvider scopeProvider)
    {
        _scopeProvider = scopeProvider;
    }

    public Token? GetToken(string serviceAlias)
    {
        using IScope scope = _scopeProvider.CreateScope();

        TokenDto entity = scope.Database.FirstOrDefault<TokenDto>("where serviceAlias = @0", serviceAlias);

        return entity != null
            ? new Token(entity.AccessToken, entity.RefreshToken, entity.ExpiresOn)
            : null;
    }

    public void SaveToken(string serviceAlias, Token token)
    {
        using IScope scope = _scopeProvider.CreateScope();

        TokenDto entity = scope.Database.SingleOrDefault<TokenDto>("where serviceAlias = @0", serviceAlias);

        bool insert = entity == null;
        entity ??= new TokenDto { ServiceAlias = serviceAlias };

        entity.AccessToken = token.AccessToken;
        entity.RefreshToken = token.RefreshToken;
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
