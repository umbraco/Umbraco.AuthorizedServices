using Microsoft.Extensions.Options;
using Umbraco.AuthorizedServices.Api.Management.Controllers.Service;
using Umbraco.AuthorizedServices.Configuration;
using Umbraco.AuthorizedServices.Services;

namespace Umbraco.AuthorizedServices.Api.Management.Controllers.Service
{
    public class AuthorizedServiceStorageController : AuthorizedServiceControllerBase
    {
        protected IOAuth2TokenStorage OAuth2TokenStorage { get; }
        protected IOAuth1TokenStorage OAuth1TokenStorage { get; }
        protected IKeyStorage KeyStorage { get; }

        public AuthorizedServiceStorageController(
            IOptionsMonitor<ServiceDetail> serviceDetailOptions,
            IOAuth2TokenStorage oauth2TokenStorage,
            IOAuth1TokenStorage oauth1TokenStorage,
            IKeyStorage keyStorage)
            : base (serviceDetailOptions)
        {
            OAuth2TokenStorage = oauth2TokenStorage;
            OAuth1TokenStorage = oauth1TokenStorage;
            KeyStorage = keyStorage;
        }

        protected async Task<bool> CheckAuthorizationStatus(ServiceDetail serviceDetail) => serviceDetail.AuthenticationMethod switch
        {
            AuthenticationMethod.OAuth1 => await StoredOAuth1TokenExists(serviceDetail),
            AuthenticationMethod.OAuth2AuthorizationCode => await StoredOAuth2TokenExists(serviceDetail),
            AuthenticationMethod.OAuth2ClientCredentials => await StoredOAuth2TokenExists(serviceDetail),
            AuthenticationMethod.ApiKey => !string.IsNullOrEmpty(serviceDetail.ApiKey)
                                           || await KeyStorage.GetKeyAsync(serviceDetail.Alias) is not null,
            _ => false
        };

        protected async Task<bool> StoredOAuth2TokenExists(ServiceDetail serviceDetail) => await OAuth2TokenStorage.GetTokenAsync(serviceDetail.Alias) != null;

        protected async Task<bool> StoredOAuth1TokenExists(ServiceDetail serviceDetail) => await OAuth1TokenStorage.GetTokenAsync(serviceDetail.Alias) != null;
    }
}
