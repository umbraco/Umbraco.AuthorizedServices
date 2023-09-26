using Microsoft.Extensions.DependencyInjection;
using Umbraco.AuthorizedServices.Configuration;
using Umbraco.AuthorizedServices.Manifests;
using Umbraco.AuthorizedServices.Services;
using Umbraco.AuthorizedServices.Services.Implement;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Extensions;

namespace Umbraco.AuthorizedServices;

internal class AuthorizedServicesComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        ConfigureOptions(builder);
        RegisterManifestFilter(builder);
        RegisterServices(builder);
    }

    private static void ConfigureOptions(IUmbracoBuilder builder)
    {
        builder.Services.AddOptions<AuthorizedServiceSettings>()
            .BindConfiguration("Umbraco:AuthorizedServices")
            .Configure(options =>
            {
                // Automatically set the alias based on the key.
                foreach (KeyValuePair<string, ServiceSummary> service in options.Services)
                {
                    service.Value.Alias = service.Key;
                }
            });

        builder.Services.ConfigureOptions<ConfigureServiceDetail>();
    }

    private static void RegisterManifestFilter(IUmbracoBuilder builder) =>
        builder.ManifestFilters().Append<AuthorizedServicesManifestFilter>();

    private static void RegisterServices(IUmbracoBuilder builder)
    {
        builder.Services.AddUnique<IAuthorizationClientFactory, AuthorizationClientFactory>();
        builder.Services.AddUnique<IAuthorizationParametersBuilder, AuthorizationParametersBuilder>();
        builder.Services.AddUnique<IExchangeTokenParametersBuilder, ExchangeTokenParametersBuilder>();
        builder.Services.AddUnique<IAuthorizationRequestSender, AuthorizationRequestSender>();
        builder.Services.AddUnique<IAuthorizedServiceAuthorizer, AuthorizedServiceAuthorizer>();
        builder.Services.AddUnique<IAuthorizationUrlBuilder, AuthorizationUrlBuilder>();
        builder.Services.AddUnique<IAuthorizedRequestBuilder, AuthorizedRequestBuilder>();

        builder.Services.AddUnique<IAuthorizedServiceCaller, AuthorizedServiceCaller>();
        builder.Services.AddUnique<IDateTimeProvider, DateTimeProvider>();
        builder.Services.AddUnique<IRefreshTokenParametersBuilder, RefreshTokenParametersBuilder>();

        builder.Services.AddUnique<IAuthorizationPayloadCache, AuthorizationPayloadCache>();
        builder.Services.AddUnique<IAuthorizationPayloadBuilder, AuthorizationPayloadBuilder>();

        builder.Services.AddUnique<ISecretEncryptor, DataProtectionSecretEncryptor>();

        builder.Services.AddUnique<ITokenFactory, TokenFactory>();
        builder.Services.AddUnique<ITokenStorage, DatabaseTokenStorage>();
        builder.Services.AddUnique<IKeyStorage, DatabaseKeyStorage>();

        builder.Services.AddSingleton<JsonSerializerFactory>();
    }
}
