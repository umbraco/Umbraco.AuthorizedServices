using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Migrations;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations;
using Umbraco.Cms.Infrastructure.Migrations.Upgrade;

namespace Umbraco.AuthorizedServices.Migrations;

public class DatabaseTokenStorageHandler : INotificationHandler<UmbracoApplicationStartingNotification>
{
    private readonly ICoreScopeProvider _coreScopeProvider;
    private readonly IMigrationPlanExecutor _migrationPlanExecutor;
    private readonly IKeyValueService _keyValueService;
    private readonly IRuntimeState _runtimeState;

    public DatabaseTokenStorageHandler(
        ICoreScopeProvider coreScopeProvider,
        IMigrationPlanExecutor migrationPlanExecutor,
        IKeyValueService keyValueService,
        IRuntimeState runtimeState)
    {
        _coreScopeProvider = coreScopeProvider;
        _migrationPlanExecutor = migrationPlanExecutor;
        _keyValueService = keyValueService;
        _runtimeState = runtimeState;
    }

    public void Handle(UmbracoApplicationStartingNotification notification)
    {
        if (_runtimeState.Level != RuntimeLevel.Run)
        {
            return;
        }

        var migrationPlan = new MigrationPlan(Constants.Migrations.MigrationPlan);

        migrationPlan.From(string.Empty)
                .To<AddDatabaseTokenStorageTable>(Constants.Migrations.TargetState);

        var upgrader = new Upgrader(migrationPlan);

        upgrader.Execute(_migrationPlanExecutor, _coreScopeProvider, _keyValueService);
    }
}
