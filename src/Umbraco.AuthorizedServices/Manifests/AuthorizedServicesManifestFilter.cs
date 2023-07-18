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

        // The PackageId property was added in Umbraco 12, so we have to use reflection here to set it if available
        // as the package depdendency is on Umbraco 10.
        // If and when we release a version with a depdendency on 12+, this should be removed and replaced with
        // a standard property setter.
        ReflectionHelper.SetOptionalPropertyValue(manifest, "PackageId", Constants.PackageId);

        manifests.Add(manifest);
    }
}
