using Umbraco.AuthorizedServices.Extensions;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;

namespace Umbraco.AuthorizedServices
{
    public class AuthorizedServicesComposer : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            builder.AddAuthorizedServicesSwaggerGenOptions();
        }
    }
}
