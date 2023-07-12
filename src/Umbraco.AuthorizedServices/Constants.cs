using System.Reflection;
using Umbraco.Cms.Core.Semver;
using Umbraco.Extensions;

namespace Umbraco.AuthorizedServices;

public static class Constants
{
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

    public static class Migrations
    {
        public const string TableName = "umbracoAuthorizedServiceToken";

        public const string MigrationPlan = "AuthorizedServicesDatabaseMigration";

        public const string TargetState = "authorizedServices-db";
    }
}
