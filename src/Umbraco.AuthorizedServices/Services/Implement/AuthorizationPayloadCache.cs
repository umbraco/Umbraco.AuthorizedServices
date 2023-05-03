using System.Collections.Concurrent;
using Umbraco.AuthorizedServices.Models;

namespace Umbraco.AuthorizedServices.Services.Implement
{
    internal class AuthorizationPayloadCache : IAuthorizationPayloadCache
    {
        protected const string CacheItemName = "{0}-payload";

        private readonly ConcurrentDictionary<string, AuthorizationPayload> _internalCache = new();

        public void Add(string key, AuthorizationPayload value) =>
            _internalCache.AddOrUpdate(string.Format(CacheItemName, key), value, (key, oldValue) => value);

        public AuthorizationPayload? Get(string key) => _internalCache.GetValueOrDefault(string.Format(CacheItemName, key));

        public void Remove(string key) => _internalCache.TryRemove(string.Format(CacheItemName, key), out _);

        public void Clear() => _internalCache.Clear();
    }
}
