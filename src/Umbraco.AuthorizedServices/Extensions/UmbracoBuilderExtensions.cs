using Microsoft.AspNetCore.OpenApi;
using Umbraco.Cms.Api.Common.OpenApi;
using Umbraco.Cms.Api.Management.OpenApi;
using Umbraco.Cms.Core.DependencyInjection;

namespace Umbraco.AuthorizedServices.Extensions
{
    public static class UmbracoBuilderExtensions
    {
        public static IUmbracoBuilder AddAuthorizedServicesOpenApiDocument(this IUmbracoBuilder builder)
        {
            builder.AddBackOfficeOpenApiDocument(
                Constants.ManagementApi.ApiName,
                document => document
                    .WithTitle(Constants.ManagementApi.ApiTitle)
                    .WithBackOfficeAuthentication()
                    .ConfigureOpenApiOptions(options => options.AddDocumentTransformer((doc, _, _) =>
                    {
                        doc.Info.Version = "Latest";
                        doc.Info.Description = $"Describes the {Constants.ManagementApi.ApiTitle} available for handling services.";
                        return Task.CompletedTask;
                    })));

            return builder;
        }
    }
}
