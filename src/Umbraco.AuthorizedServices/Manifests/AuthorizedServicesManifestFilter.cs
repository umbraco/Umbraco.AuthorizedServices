using Umbraco.Cms.Core.Manifest;

namespace Umbraco.AuthorizedServices.Manifests;

public class AuthorizedServicesManifestFilter : IManifestFilter
{
    public void Filter(List<PackageManifest> manifests)
    {
        manifests.Add(new PackageManifest
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
        });
    }
}
