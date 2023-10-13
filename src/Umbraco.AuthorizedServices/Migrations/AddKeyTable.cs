using Microsoft.Extensions.Logging;
using Umbraco.AuthorizedServices.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Migrations;

namespace Umbraco.AuthorizedServices.Migrations;

public class AddKeyTable : MigrationBase
{
    public AddKeyTable(IMigrationContext context) : base(context)
    {
    }

    protected override void Migrate()
    {
        Logger.LogDebug($"Running migration {nameof(AddKeyTable)}");

        if (TableExists(Constants.Database.TableNames.ApiKey))
        {
            Logger.LogDebug($"The database table {Constants.Database.TableNames.ApiKey} already exists, skipping.");
        }
        else
        {
            Create.Table<KeyDto>().Do();
        }
    }
}
