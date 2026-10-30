using Microsoft.Extensions.Logging;
using Umbraco.Cms.Infrastructure.Migrations;

namespace Umbraco.AuthorizedServices.Migrations;

public class RenameOAuth2TokenTable : AsyncMigrationBase
{
    public RenameOAuth2TokenTable(IMigrationContext context) : base(context)
    {
    }

    protected override Task MigrateAsync()
    {
        Logger.LogDebug($"Running migration {nameof(RenameOAuth2TokenTable)}");

        const string OriginalTableName = "umbracoAuthorizedServiceToken";
        if (TableExists(OriginalTableName) && !TableExists(Constants.Database.TableNames.OAuth2Token))
        {
            Rename.Table(OriginalTableName).To(Constants.Database.TableNames.OAuth2Token);
        }

        return Task.CompletedTask;
    }
}
