using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.AuthorizedServices.Configuration;
using Umbraco.AuthorizedServices.Manifests;
using Umbraco.AuthorizedServices.Migrations;
using Umbraco.AuthorizedServices.Services;
using Umbraco.AuthorizedServices.Services.Implement;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Extensions;

namespace Umbraco.AuthorizedServices;

internal class AuthorizedServicesComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        IConfigurationSection configSection = builder.Config.GetSection("Umbraco:AuthorizedServices");
        builder.Services.Configure<AuthorizedServiceSettings>(configSection);

        // manifest filter
        builder.ManifestFilters().Append<AuthorizedServicesManifestFilter>();

        builder.Services.AddUnique<IAuthorizationClientFactory, AuthorizationClientFactory>();
        builder.Services.AddUnique<IAuthorizationParametersBuilder, AuthorizationParametersBuilder>();
        builder.Services.AddUnique<IAuthorizationRequestSender, AuthorizationRequestSender>();
        builder.Services.AddUnique<IAuthorizedServiceAuthorizer, AuthorizedServiceAuthorizer>();
        builder.Services.AddUnique<IAuthorizationUrlBuilder, AuthorizationUrlBuilder>();
        builder.Services.AddUnique<IAuthorizedRequestBuilder, AuthorizedRequestBuilder>();

        builder.Services.AddUnique<IAuthorizedServiceCaller, AuthorizedServiceCaller>();
        builder.Services.AddUnique<IDateTimeProvider, DateTimeProvider>();
        builder.Services.AddUnique<IRefreshTokenParametersBuilder, RefreshTokenParametersBuilder>();

        builder.Services.AddUnique<IAuthorizationPayloadCache, AuthorizationPayloadCache>();
        builder.Services.AddUnique<IAuthorizationPayloadBuilder, AuthorizationPayloadBuilder>();

        builder.Services.AddUnique<ISecretEncryptor>(factory =>
            {
                var tokenEncryptionKey = configSection.GetValue<string>(nameof(AuthorizedServiceSettings.TokenEncryptionKey));

                return new AesSecretEncryptor(tokenEncryptionKey ?? string.Empty);
            });

        builder.Services.AddUnique<ITokenFactory, TokenFactory>();
        builder.Services.AddUnique<ITokenStorage, DatabaseTokenStorage>();

        builder.Services.AddSingleton<JsonSerializerFactory>();

        builder.AddNotificationHandler<UmbracoApplicationStartingNotification, DatabaseMigrationHandler>();
    }
}
