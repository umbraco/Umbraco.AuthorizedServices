using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Umbraco.AuthorizedServices.Api.Configuration;
using Umbraco.Cms.Api.Common.OpenApi;
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

                options.OperationFilter<BackOfficeSecurityRequirementsOperationFilter>();
            })
            .AddSingleton<IOperationIdHandler, AuthorizedServicesOperationIdHandler>();

            return builder;
        }
    }
}
