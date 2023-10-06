namespace Umbraco.AuthorizedServices.Services;

public interface ITokenCache
{
    string? Get(string serviceAlias);

    string? GetByValue(string token);

    void Save(string serviceAlias, string token);

    void Delete(string serviceAlias);
}
