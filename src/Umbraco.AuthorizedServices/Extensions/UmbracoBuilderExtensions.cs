using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Umbraco.Cms.Core.DependencyInjection;

namespace Umbraco.AuthorizedServices.Extensions
{
    public static class UmbracoBuilderExtensions
    {
        public static IUmbracoBuilder AddAuthorizedServicesSwaggerGenOptions(this IUmbracoBuilder builder)
        {
            builder.Services.Configure<SwaggerGenOptions>(options =>
            {
                options.SwaggerDoc(
                    Constants.ManagementApi.ApiName,
                    new OpenApiInfo
                    {
                        Title = Constants.ManagementApi.ApiTitle,
                        Version = "Latest",
                        Description = $"Describes the {Constants.ManagementApi.ApiTitle} available for handling services."
                    });

                options.CustomOperationIds(e => $"{e.ActionDescriptor.RouteValues["action"]}");
            });

            return builder;
        }
    }
}
