using Umbraco.AuthorizedServices.Models;

namespace Umbraco.AuthorizedServices.Services.Implement;

internal sealed class DatabaseTokenStorage : ITokenStorage
{
    public Token? GetToken(string serviceAlias) => throw new NotImplementedException();

    public void SaveToken(string serviceAlias, Token token) => throw new NotImplementedException();

    public void DeleteToken(string serviceAlias) => throw new NotImplementedException();
}
