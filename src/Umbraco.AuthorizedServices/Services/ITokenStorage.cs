using Umbraco.AuthorizedServices.Models;

namespace Umbraco.AuthorizedServices.Services;

public interface ITokenStorage
{
    Token? GetToken(string serviceAlias);

    void SaveToken(string serviceAlias, Token token);

    void DeleteToken(string serviceAlias);
}
