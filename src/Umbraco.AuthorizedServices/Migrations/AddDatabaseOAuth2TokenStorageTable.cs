using Microsoft.Extensions.Logging;
using Umbraco.AuthorizedServices.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Migrations;

namespace Umbraco.AuthorizedServices.Migrations;

public class AddDatabaseOAuth2TokenStorageTable : MigrationBase
{
    public AddDatabaseOAuth2TokenStorageTable(IMigrationContext context) : base(context)
    {
    }

    protected override void Migrate()
    {
        Logger.LogDebug($"Running migration {nameof(AddDatabaseOAuth2TokenStorageTable)}");

        if (TableExists(Constants.Migrations.UmbracoAuthorizedServiceOAuth2TokenTableName))
        {
            Logger.LogDebug($"The database table {Constants.Migrations.UmbracoAuthorizedServiceOAuth2TokenTableName} already exists, skipping.");
        }
        else
        {
            Create.Table<OAuth2TokenDto>().Do();
        }
    }
}
