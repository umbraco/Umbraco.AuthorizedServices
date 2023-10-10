using Microsoft.Extensions.Logging;
using Umbraco.AuthorizedServices.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Migrations;

namespace Umbraco.AuthorizedServices.Migrations;

public class AddDatabaseOAuth1TokenStorageTable : MigrationBase
{
    public AddDatabaseOAuth1TokenStorageTable(IMigrationContext context) : base(context)
    {
    }

    protected override void Migrate()
    {
        Logger.LogDebug($"Running migration {nameof(AddDatabaseOAuth1TokenStorageTable)}");

        if (TableExists(Constants.Migrations.UmbracoAuthorizedServiceOAuth1TargetState))
        {
            Logger.LogDebug($"The database table {Constants.Migrations.UmbracoAuthorizedServiceOAuth1TargetState} already exists, skipping.");
        }
        else
        {
            Create.Table<OAuth1TokenDto>().Do();
        }
    }
}
