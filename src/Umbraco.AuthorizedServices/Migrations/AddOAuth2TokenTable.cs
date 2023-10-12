using Microsoft.Extensions.Logging;
using Umbraco.AuthorizedServices.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Migrations;

namespace Umbraco.AuthorizedServices.Migrations;

public class AddOAuth2TokenTable : MigrationBase
{
    public AddOAuth2TokenTable(IMigrationContext context) : base(context)
    {
    }

    protected override void Migrate()
    {
        Logger.LogDebug($"Running migration {nameof(AddOAuth2TokenTable)}");

        if (TableExists(Constants.Database.TableNames.OAuth2Token))
        {
            Logger.LogDebug($"The database table {Constants.Database.TableNames.OAuth2Token} already exists, skipping.");
        }
        else
        {
            Create.Table<OAuth2TokenDto>().Do();
        }
    }
}
