namespace Umbraco.AuthorizedServices;

public static class Constants
{
    public const string PluginName = "UmbracoAuthorizedServices";

    public const string Separator = "-";

    public static class Trees
    {
        public const string AuthorizedServices = nameof(AuthorizedServices);
    }

    public static class Migrations
    {
        public const string TableName = "umbracoAuthorizedServiceToken";

        public const string MigrationPlan = "AuthorizedServicesDatabaseMigration";

        public const string TargetState = "authorizedServices-db";
    }
}
