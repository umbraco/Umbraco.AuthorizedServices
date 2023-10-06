using System.Reflection;
using Umbraco.Cms.Core.Semver;
using Umbraco.Extensions;

namespace Umbraco.AuthorizedServices;

public static class Constants
{
    internal const string PackageId = "Umbraco.AuthorizedServices";

    internal const string PackageName = "Umbraco Authorized Services";

    public const string PluginName = "UmbracoAuthorizedServices";

    public const string Separator = "-";

    public static readonly string InformationalVersion = GetInformationalVersion();

    private static string GetInformationalVersion()
    {
        Assembly assembly = typeof(Constants).Assembly;
        AssemblyInformationalVersionAttribute? assemblyInformationalVersionAttribute = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
        if (assemblyInformationalVersionAttribute is not null &&
            SemVersion.TryParse(assemblyInformationalVersionAttribute.InformationalVersion, out SemVersion? semVersion))
        {
            return semVersion!.ToSemanticStringWithoutBuild();
        }
        else
        {
            AssemblyName assemblyName = assembly.GetName();
            if (assemblyName.Version is not null)
            {
                return assemblyName.Version.ToString(3);
            }
        }

        return string.Empty;
    }

    public static class Trees
    {
        public const string AuthorizedServices = nameof(AuthorizedServices);
    }

    public static class OAuth1a
    {
        public const string OAuthToken = "oauth_token";

        public const string OAuthTokenSecret = "oauth_token_secret";

        public const string OAuthVerifier = "oauth_verifier";
    }

    public static class Migrations
    {
        public const string UmbracoAuthorizedServiceTokenTableName = "umbracoAuthorizedServiceToken";

        public const string UmbracoAuthorizedServiceKeyTableName = "umbracoAuthorizedServiceKey";

        public const string UmbracoAuthorizedServiceOAuth1aTokenTableName = "umbracoAuthorizedServiceOAuth1aToken";

        public const string MigrationPlan = "AuthorizedServicesDatabaseMigration";

        public const string UmbracoAuthorizedServiceTokenTargetState = "authorizedServices-token-db";

        public const string UmbracoAuthorizedServiceKeyTargetState = "authorizedServices-key-db";

        public const string UmbracoAuthorizedServiceOAuth1aTargetState = "authorizedServices-oauth1a-db";
    }
}
