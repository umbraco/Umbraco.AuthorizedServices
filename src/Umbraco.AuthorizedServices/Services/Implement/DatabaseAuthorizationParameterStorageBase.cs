using Microsoft.Extensions.Logging;
using Umbraco.Cms.Infrastructure.Scoping;

namespace Umbraco.AuthorizedServices.Services.Implement;

internal class DatabaseAuthorizationParameterStorageBase
{
    public DatabaseAuthorizationParameterStorageBase(
        IScopeProvider scopeProvider,
        ISecretEncryptor encryptor,
        ILogger<DatabaseAuthorizationParameterStorageBase> logger)
    {
        ScopeProvider = scopeProvider;
        Encryptor = encryptor;
        Logger = logger;
    }

    protected IScopeProvider ScopeProvider { get; }

    protected ISecretEncryptor Encryptor { get; }

    protected ILogger<DatabaseAuthorizationParameterStorageBase> Logger { get; }
}
