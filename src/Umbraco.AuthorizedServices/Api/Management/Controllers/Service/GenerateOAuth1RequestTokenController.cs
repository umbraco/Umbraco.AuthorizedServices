using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Umbraco.AuthorizedServices.Configuration;
using Umbraco.AuthorizedServices.Exceptions;
using Umbraco.AuthorizedServices.Helpers;
using Umbraco.AuthorizedServices.Models.Request;
using Umbraco.AuthorizedServices.Services;
using Umbraco.Cms.Core.Cache;
using Umbraco.Extensions;

namespace Umbraco.AuthorizedServices.Api.Management.Controllers.Service
{
    [ApiVersion("1.0")]
    [ApiExplorerSettings(GroupName = Constants.ManagementApi.ServiceGroupName)]
    public class GenerateOAuth1RequestTokenController : AuthorizedServiceControllerBase
    {
        private readonly AppCaches _appCaches;
        private readonly IAuthorizationUrlBuilder _authorizationUrlBuilder;
        private readonly IAuthorizedServiceAuthorizer _serviceAuthorizer;

        public GenerateOAuth1RequestTokenController(
            IOptionsMonitor<ServiceDetail> serviceDetailOptions,
            AppCaches appCaches,
            IAuthorizationUrlBuilder authorizationUrlBuilder,
            IAuthorizedServiceAuthorizer serviceAuthorizer)
            : base(serviceDetailOptions)
        {
            _appCaches = appCaches;
            _authorizationUrlBuilder = authorizationUrlBuilder;
            _serviceAuthorizer = serviceAuthorizer;
        }

        [HttpPost("oauth1/request-token")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [HttpPost]
        public async Task<IActionResult> GenerateOAuth1RequestToken(GenerateToken model)
        {
            ServiceDetail serviceDetail = ServiceDetailOptions.Get(model.Alias);

            var url = _authorizationUrlBuilder.BuildOAuth1RequestTokenUrl(serviceDetail, HttpContext, HttpMethod.Post, OAuth1Helper.GetNonce(), OAuth1Helper.GetTimestamp());

            Models.AuthorizationResult requestTokenResponse = await _serviceAuthorizer.GenerateOAuth1RequestTokenAsync(model.Alias, url);

            if (requestTokenResponse.Success && requestTokenResponse.Result is not null && requestTokenResponse.Result.TryParseOAuth1Response(out var oauthToken, out var oauthTokenSecret))
            {
                _appCaches.RuntimeCache.InsertCacheItem(oauthToken, () => serviceDetail.Alias);

                _appCaches.RuntimeCache.InsertCacheItem(CacheHelper.GetTokenSecretCacheKey(serviceDetail.Alias), () => oauthTokenSecret);

                return Ok(string.Format(
                    "{0}{1}?{2}",
                    serviceDetail.IdentityHost,
                    serviceDetail.RequestIdentityPath,
                    requestTokenResponse.Result));
            }

            throw new AuthorizedServiceException("Failed to obtain request token");
        }
    }
}
