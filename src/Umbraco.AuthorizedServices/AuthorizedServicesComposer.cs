using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.AuthorizedServices.Configuration;
using Umbraco.AuthorizedServices.Manifests;
using Umbraco.AuthorizedServices.Migrations;
using Umbraco.AuthorizedServices.Services;
using Umbraco.AuthorizedServices.Services.Implement;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Extensions;

namespace Umbraco.AuthorizedServices;

internal class AuthorizedServicesComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        const string ConfigurationRoot = "Umbraco:AuthorizedServices";
        IConfigurationSection configSection = builder.Config.GetSection(ConfigurationRoot);
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
                ILogger<AuthorizedServicesComposer> logger = factory.GetRequiredService<ILogger<AuthorizedServicesComposer>>();
                var tokenEncryptionKey = configSection.GetValue<string>(nameof(AuthorizedServiceSettings.TokenEncryptionKey));

                if (string.IsNullOrWhiteSpace(tokenEncryptionKey))
                {
                    logger.LogWarning($"No encryption key was found in configuration at {ConfigurationRoot}:{nameof(AuthorizedServiceSettings.TokenEncryptionKey)}. Falling back to using the Umbraco:CMS:{nameof(GlobalSettings)}:{nameof(GlobalSettings.Id)} value.");

                    IOptions<GlobalSettings> globalSettings = factory.GetRequiredService<IOptions<GlobalSettings>>();
                    tokenEncryptionKey = globalSettings.Value.Id;

                    return new NoopSecretEncryptor();
                }

                if (string.IsNullOrWhiteSpace(tokenEncryptionKey))
                {
                    logger.LogWarning($"Could not fallback back to using the Umbraco:CMS:{nameof(GlobalSettings)}:{nameof(GlobalSettings.Id)} value as it has not been completed. Access tokens will not be encrypted when stored in the local database.");
                    return new NoopSecretEncryptor();
                }

                return new AesSecretEncryptor(tokenEncryptionKey);
            });

        builder.Services.AddUnique<ITokenFactory, TokenFactory>();
        builder.Services.AddUnique<ITokenStorage, DatabaseTokenStorage>();

        builder.Services.AddSingleton<JsonSerializerFactory>();

        builder.AddNotificationHandler<UmbracoApplicationStartingNotification, DatabaseMigrationHandler>();
    }
}
