using Microsoft.Extensions.Logging;
using Umbraco.AuthorizedServices.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Migrations;

namespace Umbraco.AuthorizedServices.Migrations;

public class AddDatabaseTokenStorageTable : MigrationBase
{
    public AddDatabaseTokenStorageTable(IMigrationContext context) : base(context)
    {
    }

    protected override void Migrate()
    {
        Logger.LogDebug($"Running migration {nameof(AddDatabaseTokenStorageTable)}");

        if (TableExists(Constants.Migrations.UmbracoAuthorizedServiceTokenTableName))
        {
            Logger.LogDebug($"The database table {Constants.Migrations.UmbracoAuthorizedServiceTokenTableName} already exists, skipping.");
        }
        else
        {
            Create.Table<TokenDto>().Do();
        }
    }
}
