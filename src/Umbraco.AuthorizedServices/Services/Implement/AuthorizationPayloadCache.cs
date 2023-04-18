using System.Collections.Concurrent;

namespace Umbraco.AuthorizedServices.Services.Implement
{
    internal class AuthorizationPayloadCache : IAuthorizationPayloadCache
    {
        protected const string CacheItemName = "{0}-payload";

        private readonly ConcurrentDictionary<string, object> _internalCache = new();

        public void Add(string key, object value) => _internalCache.GetOrAdd(string.Format(CacheItemName, key), value);

        public object? Get(string key) => _internalCache.GetValueOrDefault(string.Format(CacheItemName, key));

        public void Remove(string key) => _internalCache.TryRemove(string.Format(CacheItemName, key), out _);

        public void Clear() => _internalCache.Clear();
    }
}
