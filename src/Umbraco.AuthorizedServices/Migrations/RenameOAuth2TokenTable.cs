using Microsoft.Extensions.Logging;
using Umbraco.Cms.Infrastructure.Migrations;

namespace Umbraco.AuthorizedServices.Migrations;

public class RenameOAuth2TokenTable : MigrationBase
{
    public RenameOAuth2TokenTable(IMigrationContext context) : base(context)
    {
    }

    protected override void Migrate()
    {
        Logger.LogDebug($"Running migration {nameof(RenameOAuth2TokenTable)}");

        const string OriginalTableName = "umbracoAuthorizedServiceToken";
        if (TableExists(OriginalTableName) && !TableExists(Constants.Database.TableNames.OAuth2Token))
        {
            Rename.Table(OriginalTableName).To(Constants.Database.TableNames.OAuth2Token);
        }
    }
}
