using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Umbraco.AuthorizedServices.Configuration;
using Umbraco.AuthorizedServices.Helpers;
using Umbraco.AuthorizedServices.Models.Request;
using Umbraco.AuthorizedServices.Services;
using Umbraco.Cms.Core.Cache;

namespace Umbraco.AuthorizedServices.Api.Management.Controllers.Service
{
    [ApiVersion("1.0")]
    [ApiExplorerSettings(GroupName = Constants.ManagementApi.ServiceGroupName)]
    public class RevokeAccessController : AuthorizedServiceStorageController
    {
        private readonly AppCaches _appCaches;

        public RevokeAccessController(
            IOptionsMonitor<ServiceDetail> serviceDetailOptions,
            IOAuth2TokenStorage oauth2TokenStorage,
            IOAuth1TokenStorage oauth1TokenStorage,
            IKeyStorage keyStorage,
            AppCaches appCaches)
            : base(serviceDetailOptions, oauth2TokenStorage, oauth1TokenStorage, keyStorage)
        {
            _appCaches = appCaches;
        }

        [HttpPost("revoke")]
        public async Task<IActionResult> RevokeAccess(RevokeAccess model)
        {
            ServiceDetail serviceDetail = ServiceDetailOptions.Get(model.Alias);

            switch (serviceDetail.AuthenticationMethod)
            {
                case AuthenticationMethod.ApiKey:
                    await KeyStorage.DeleteKeyAsync(model.Alias);
                    ClearCachedApiKey(model.Alias);
                    break;
                case AuthenticationMethod.OAuth1:
                    await OAuth1TokenStorage.DeleteTokenAsync(model.Alias);
                    ClearCachedToken(model.Alias);
                    break;
                case AuthenticationMethod.OAuth2AuthorizationCode:
                case AuthenticationMethod.OAuth2ClientCredentials:
                    await OAuth2TokenStorage.DeleteTokenAsync(model.Alias);
                    ClearCachedToken(model.Alias);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(serviceDetail.AuthenticationMethod));
            }

            return Ok();
        }

        private void ClearCachedApiKey(string serviceAlias)
        {
            var cacheKey = CacheHelper.GetApiKeyCacheKey(serviceAlias);
            _appCaches.RuntimeCache.ClearByKey(cacheKey);
        }

        private void ClearCachedToken(string serviceAlias)
        {
            var cacheKey = CacheHelper.GetTokenCacheKey(serviceAlias);
            _appCaches.RuntimeCache.ClearByKey(cacheKey);
        }
    }
}
