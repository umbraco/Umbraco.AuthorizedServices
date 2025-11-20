using Microsoft.Extensions.Logging;
using NPoco;
using Umbraco.AuthorizedServices.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Scoping;

namespace Umbraco.AuthorizedServices.Services.Implement;

/// <summary>
/// Implements <see cref="IKeyStorage"/> for API key storage using a database table.
/// </summary>
internal sealed class DatabaseKeyStorage : DatabaseAuthorizationParameterStorageBase, IKeyStorage
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DatabaseKeyStorage"/> class.
    /// </summary>
    public DatabaseKeyStorage(IScopeProvider scopeProvider, ISecretEncryptor encryptor, ILogger<DatabaseKeyStorage> logger)
        : base(scopeProvider, encryptor, logger)
    {
    }

    /// <inheritdoc/>
    public async Task<string?> GetKeyAsync(string serviceAlias)
    {
        using IScope scope = ScopeProvider.CreateScope();

        KeyDto entity = await scope.Database.FirstOrDefaultAsync<KeyDto>("where serviceAlias = @0", serviceAlias);
        if (entity == null)
        {
            return null;
        }

        if (!Encryptor.TryDecrypt(entity.ApiKey, out string apiKey))
        {
            await RemoveCorruptApiKey(serviceAlias);
            return null;
        }

        return apiKey;
    }

    /// <inheritdoc/>
    public async Task SaveKeyAsync(string serviceAlias, string key)
    {
        using IScope scope = ScopeProvider.CreateScope();

        KeyDto entity = await scope.Database.SingleOrDefaultAsync<KeyDto>("where serviceAlias = @0", serviceAlias);

        bool insert = entity == null;
        entity ??= new KeyDto { ServiceAlias = serviceAlias };

        entity.ApiKey = Encryptor.Encrypt(key);

        if (insert)
        {
            await scope.Database.InsertAsync(entity);
        }
        else
        {
            await scope.Database.UpdateAsync(entity);
        }

        scope.Complete();
    }

    /// <inheritdoc/>
    public async Task DeleteKeyAsync(string serviceAlias)
    {
        using IScope scope = ScopeProvider.CreateScope();

        KeyDto? entity = await scope.Database.SingleOrDefaultAsync<KeyDto>("where serviceAlias = @0", serviceAlias);
        if (entity is not null)
        {
            scope.Database.Delete(entity);
        }

        scope.Complete();
    }

    private async Task RemoveCorruptApiKey(string serviceAlias)
    {
        await DeleteKeyAsync(serviceAlias);
        Logger.LogWarning($"Could not decrypt the stored API key for authorized service with alias '{serviceAlias}'. API key has been removed from storage.");
    }
}
