using Umbraco.AuthorizedServices.Helpers;
using Umbraco.Cms.Core.Manifest;

namespace Umbraco.AuthorizedServices.Manifests;

public class AuthorizedServicesManifestFilter : IManifestFilter
{
    public void Filter(List<PackageManifest> manifests)
    {
        var manifest = new PackageManifest
        {
            AllowPackageTelemetry = true,
            Version = Constants.InformationalVersion,
            PackageName = Constants.PackageName,
            BundleOptions = BundleOptions.None,
            Scripts = new[]
            {
                 "/App_Plugins/UmbracoAuthorizedServices/index.js"
            },
            Stylesheets = new[]
            {
                "/App_Plugins/UmbracoAuthorizedServices/css/style.css"
            }
        };

        ReflectionHelper.SetOptionalPropertyValue(manifest, "PackageId", Constants.PackageId);

        manifests.Add(manifest);
    }
}
