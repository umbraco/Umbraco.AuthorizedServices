using Umbraco.AuthorizedServices.Models;

namespace Umbraco.AuthorizedServices.Services;

public interface IAuthorizationPayloadCache
{
    void Add(string key, AuthorizationPayload value);

    AuthorizationPayload? Get(string key);

    void Remove(string key);

    void Clear();
}
