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

    public static class Cache
    {
        public const string AuthorizationPayloadKeyFormat = "Umbraco_AuthorizedServices_Payload_{0}";

        public const string AuthorizationTokenFormat = "Umbraco_AuthorizedServices_Token_{0}";
    }

    public static class OAuth1
    {
        public const string OAuthToken = "oauth_token";

        public const string OAuthTokenSecret = "oauth_token_secret";

        public const string OAuthVerifier = "oauth_verifier";

        public const string OAuthNonce = "oauth_nonce";

        public const string OAuthTimestamp = "oauth_timestamp";

        public const string OAuthSignature = "oauth_signature";

        public const string OAuthSignatureMethod = "oauth_signature_method";

        public const string OAuthConsumerKey = "oauth_consumer_key";

        public const string OAuthVersion = "oauth_version";
    }

    public static class Database
    {
        public static class TableNames
        {
            public const string OAuth2Token = "umbracoAuthorizedServiceOAuth2Token";

            public const string ApiKey = "umbracoAuthorizedServiceKey";

            public const string OAuth1Token = "umbracoAuthorizedServiceOAuth1Token";
        }

        public static class Migrations
        {

            public const string MigrationPlan = "AuthorizedServicesDatabaseMigration";

            public static class TargetStates
            {
                public const string AddOAuth2TokenTable = "authorizedServices-db";

                public const string RenameOAuth2TokenTable = "authorizedServices-oauth2-rename-db";

                public const string AddKeyTable = "authorizedServices-key-db";

                public const string AddOAuth1TokenTable = "authorizedServices-oauth1_token-db";
            }
        }
    }
}
