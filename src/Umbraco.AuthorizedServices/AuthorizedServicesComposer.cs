using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.AuthorizedServices.Configuration;
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

        builder.Services.AddUnique<IAuthorizationClientFactory, AuthorizationClientFactory>();
        builder.Services.AddUnique<IAuthorizationParametersBuilder, AuthorizationParametersBuilder>();
        builder.Services.AddUnique<IAuthorizationRequestSender, AuthorizationRequestSender>();
        builder.Services.AddUnique<IAuthorizedServiceAuthorizer, AuthorizedServiceAuthorizer>();
        builder.Services.AddUnique<IAuthorizationUrlBuilder, AuthorizationUrlBuilder>();
        builder.Services.AddUnique<IAuthorizedRequestBuilder, AuthorizedRequestBuilder>();

        builder.Services.AddUnique<IAuthorizedServiceCaller, AuthorizedServiceCaller>();
        builder.Services.AddUnique<IDateTimeProvider, DateTimeProvider>();
        builder.Services.AddUnique<IRefreshTokenParametersBuilder, RefreshTokenParametersBuilder>();
        // TODO: register ISecretEncryptor with key from config:
        //// builder.Services.AddUnique<ISecretEncryptor, factory => new SecretEncryptor("")));
        builder.Services.AddUnique<ITokenFactory, TokenFactory>();
        builder.Services.AddUnique<ITokenStorage, DatabaseTokenStorage>();

        builder.AddNotificationHandler<UmbracoApplicationStartingNotification, DatabaseTokenStorageHandler>();
    }
}
