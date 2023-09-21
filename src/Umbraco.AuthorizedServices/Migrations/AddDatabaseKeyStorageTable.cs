using Microsoft.Extensions.Logging;
using Umbraco.AuthorizedServices.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Migrations;

namespace Umbraco.AuthorizedServices.Migrations;

public class AddDatabaseKeyStorageTable : MigrationBase
{
    public AddDatabaseKeyStorageTable(IMigrationContext context) : base(context)
    {
    }

    protected override void Migrate()
    {
        Logger.LogDebug($"Running migration {nameof(AddDatabaseKeyStorageTable)}");

        if (TableExists(Constants.Migrations.UmbracoAuthorizedServiceKeyTableName))
        {
            Logger.LogDebug($"The database table {Constants.Migrations.UmbracoAuthorizedServiceKeyTableName} already exists, skipping.");
        }
        else
        {
            Create.Table<KeyDto>().Do();
        }
    }
}
