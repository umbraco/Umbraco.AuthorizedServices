namespace Umbraco.AuthorizedServices;

public static class Constants
{
    public const string PluginName = "UmbracoAuthorizedServices";

    public static class Trees
    {
        public const string AuthorizedServices = nameof(AuthorizedServices);
    }

    public static class Authorization
    {
        internal const string State = "abc123"; // TODO: This needs to be a random string.
    }

    public static class Migrations
    {
        public const string TableName = "umbracoAuthorizedServiceToken";

        public const string MigrationPlan = "AuthorizedServicesDatabaseMigration";

        public const string TargetState = "authorizationToken-db";
    }
}
