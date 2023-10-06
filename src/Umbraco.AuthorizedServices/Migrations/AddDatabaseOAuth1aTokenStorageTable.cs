using Microsoft.Extensions.Logging;
using Umbraco.AuthorizedServices.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Migrations;

namespace Umbraco.AuthorizedServices.Migrations;

public class AddDatabaseOAuth1aTokenStorageTable : MigrationBase
{
    public AddDatabaseOAuth1aTokenStorageTable(IMigrationContext context) : base(context)
    {
    }

    protected override void Migrate()
    {
        Logger.LogDebug($"Running migration {nameof(AddDatabaseOAuth1aTokenStorageTable)}");

        if (TableExists(Constants.Migrations.UmbracoAuthorizedServiceOAuth1aTargetState))
        {
            Logger.LogDebug($"The database table {Constants.Migrations.UmbracoAuthorizedServiceOAuth1aTargetState} already exists, skipping.");
        }
        else
        {
            Create.Table<OAuth1aTokenDto>().Do();
        }
    }
}
