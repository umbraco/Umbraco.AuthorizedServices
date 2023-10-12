using Microsoft.Extensions.Logging;
using Umbraco.AuthorizedServices.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Migrations;

namespace Umbraco.AuthorizedServices.Migrations;

public class AddOAuth1TokenTable : MigrationBase
{
    public AddOAuth1TokenTable(IMigrationContext context) : base(context)
    {
    }

    protected override void Migrate()
    {
        Logger.LogDebug($"Running migration {nameof(AddOAuth1TokenTable)}");

        if (TableExists(Constants.Database.TableNames.OAuth1Token))
        {
            Logger.LogDebug($"The database table {Constants.Database.TableNames.OAuth1Token} already exists, skipping.");
        }
        else
        {
            Create.Table<OAuth1TokenDto>().Do();
        }
    }
}
