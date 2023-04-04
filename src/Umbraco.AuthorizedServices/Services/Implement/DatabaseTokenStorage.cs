using Umbraco.AuthorizedServices.Migrations;
using Umbraco.AuthorizedServices.Models;
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
        using var scope = _scopeProvider.CreateScope();

        var entity = scope.Database.FirstOrDefault<DatabaseTokenStorageSchema>("where serviceAlias = @0", serviceAlias);

        return entity != null
            ? new Token(entity.AccessToken, entity.RefreshToken, entity.ExpiresOn)
            : null;
    }

    public void SaveToken(string serviceAlias, Token token)
    {
        using var scope = _scopeProvider.CreateScope();

        scope.Database.Insert(new DatabaseTokenStorageSchema
        {
            ServiceAlias = serviceAlias,
            AccessToken = token.AccessToken,
            RefreshToken = token.RefreshToken ?? string.Empty,
            ExpiresOn = token.ExpiresOn ?? default,
        });

        scope.Complete();
    }

    public void DeleteToken(string serviceAlias)
    {
        using var scope = _scopeProvider.CreateScope();

        var entity = scope.Database.Single<DatabaseTokenStorageSchema>("where serviceAlias = @0", serviceAlias);

        scope.Database.Delete(entity);

        scope.Complete();
    }
}
