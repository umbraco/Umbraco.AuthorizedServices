using Microsoft.Extensions.Logging;
using Umbraco.AuthorizedServices.Models;
using Umbraco.AuthorizedServices.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Scoping;

namespace Umbraco.AuthorizedServices.Services.Implement;

/// <summary>
/// Implements <see cref="IKeyStorage"/> for API key storage using a database table.
/// </summary>
internal sealed class DatabaseKeyStorage : IKeyStorage
{
    private readonly IScopeProvider _scopeProvider;
    private readonly ISecretEncryptor _encryptor;
    private readonly ILogger<DatabaseKeyStorage> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="DatabaseKeyStorage"/> class.
    /// </summary>
    public DatabaseKeyStorage(IScopeProvider scopeProvider, ISecretEncryptor encryptor, ILogger<DatabaseKeyStorage> logger)
    {
        _scopeProvider = scopeProvider;
        _encryptor = encryptor;
        _logger = logger;
    }

    /// <inheritdoc/>
    public string? GetKey(string serviceAlias)
    {
        using IScope scope = _scopeProvider.CreateScope();

        KeyDto entity = scope.Database.FirstOrDefault<KeyDto>("where serviceAlias = @0", serviceAlias);
        if (entity == null)
        {
            return null;
        }

        if (!_encryptor.TryDecrypt(entity.ApiKey, out string apiKey))
        {
            RemoveCorruptApiKey(serviceAlias);
            return null;
        }

        return apiKey;
    }

    /// <inheritdoc/>
    public void SaveKey(string serviceAlias, string key)
    {
        using IScope scope = _scopeProvider.CreateScope();

        KeyDto entity = scope.Database.SingleOrDefault<KeyDto>("where serviceAlias = @0", serviceAlias);

        bool insert = entity == null;
        entity ??= new KeyDto { ServiceAlias = serviceAlias };

        entity.ApiKey = _encryptor.Encrypt(key);

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

    /// <inheritdoc/>
    public void DeleteKey(string serviceAlias)
    {
        using IScope scope = _scopeProvider.CreateScope();

        KeyDto entity = scope.Database.Single<KeyDto>("where serviceAlias = @0", serviceAlias);

        scope.Database.Delete(entity);

        scope.Complete();
    }

    private void RemoveCorruptApiKey(string serviceAlias)
    {
        DeleteKey(serviceAlias);
        _logger.LogWarning($"Could not decrypt the stored API key for authorized service with alias '{serviceAlias}'. API key has been removed from storage.");
    }
}