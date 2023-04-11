namespace Umbraco.AuthorizedServices.Services.Implement;

internal class StateCache
{
    public static StateCache Instance { get; private set; } = new StateCache();

    private readonly Dictionary<string, string> _stateCacheDictionary;

    protected internal StateCache()
    {
        _stateCacheDictionary = new Dictionary<string, string>();
    }

    public void Add(string key, string value)
    {
        lock (_stateCacheDictionary)
        {
            _stateCacheDictionary[key] = value;
        }
    }

    public string? Get(string key)
    {
        lock (_stateCacheDictionary)
        {
            return _stateCacheDictionary.TryGetValue(key, out var value) ? value : null;
        }
    }

    public void Remove(string key)
    {
        lock (_stateCacheDictionary)
        {
            _stateCacheDictionary.Remove(key);
        }
    }
}
