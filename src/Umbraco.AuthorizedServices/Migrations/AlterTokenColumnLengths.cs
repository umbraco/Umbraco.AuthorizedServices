using Microsoft.Extensions.Logging;
using Umbraco.Cms.Infrastructure.Migrations;
using Umbraco.Cms.Infrastructure.Persistence;

namespace Umbraco.AuthorizedServices.Migrations;

public class AlterTokenColumnLengths : MigrationBase
{
    public AlterTokenColumnLengths(IMigrationContext context) : base(context)
    {
    }

    protected override void Migrate()
    {
        Logger.LogDebug($"Running migration {nameof(AlterTokenColumnLengths)}");

        // SQLite doesn't support alter table but fortunately the fields are variable text length anyway.
        // So skip unless running on SQL Server.
        if (IsSqlite(Database.SqlContext))
        {
            Logger.LogDebug($"Skipping altering udi field column lengths in Authorized Services tables as running on SQLite.");
            return;
        }

        Alter.Table(Constants.Database.TableNames.OAuth2Token)
            .AlterColumn("accessToken")
            .AsString(Constants.Database.TokenFieldSize)
            .NotNullable()
            .Do();

        Alter.Table(Constants.Database.TableNames.OAuth2Token)
            .AlterColumn("refreshToken")
            .AsString(Constants.Database.TokenFieldSize)
            .NotNullable()
            .Do();

        Alter.Table(Constants.Database.TableNames.OAuth1Token)
            .AlterColumn("oauthToken")
            .AsString(Constants.Database.TokenFieldSize)
            .NotNullable()
            .Do();

        Alter.Table(Constants.Database.TableNames.OAuth1Token)
            .AlterColumn("oauthTokenSecret")
            .AsString(Constants.Database.TokenFieldSize)
            .NotNullable()
            .Do();
    }

    private static bool IsSqlite(ISqlContext sqlContext) => Type.GetType("Umbraco.Cms.Persistence.Sqlite.Services.SqliteSyntaxProvider, Umbraco.Cms.Persistence.Sqlite") == sqlContext.SqlSyntax.GetType();
}
