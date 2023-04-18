namespace Umbraco.AuthorizedServices.Services;

public interface IAuthorizationPayloadCache
{
    void Add(string key, object value);

    object? Get(string key);

    void Remove(string key);

    void Clear();
}
