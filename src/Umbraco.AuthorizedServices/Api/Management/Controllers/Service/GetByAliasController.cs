using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Umbraco.AuthorizedServices.Configuration;
using Umbraco.AuthorizedServices.Helpers;
using Umbraco.AuthorizedServices.Models;
using Umbraco.AuthorizedServices.Services;
using Umbraco.Cms.Core.Cache;

namespace Umbraco.AuthorizedServices.Api.Management.Controllers.Service
{
    [ApiVersion("1.0")]
    [ApiExplorerSettings(GroupName = Constants.ManagementApi.ServiceGroupName)]
    public class GetByAliasController : AuthorizedServiceStorageController
    {
        private readonly IAuthorizationPayloadBuilder _authorizationPayloadBuilder;
        private readonly AppCaches _appCaches;
        private readonly IAuthorizationUrlBuilder _authorizationUrlBuilder;

        public GetByAliasController(
            IOptionsMonitor<ServiceDetail> serviceDetailOptions,
            IOAuth2TokenStorage oauth2TokenStorage,
            IOAuth1TokenStorage oauth1TokenStorage,
            IKeyStorage keyStorage,
            IAuthorizationPayloadBuilder authorizationPayloadBuilder,
            AppCaches appCaches,
            IAuthorizationUrlBuilder authorizationUrlBuilder)
            : base(serviceDetailOptions, oauth2TokenStorage, oauth1TokenStorage, keyStorage)
        {
            _authorizationPayloadBuilder = authorizationPayloadBuilder;
            _appCaches = appCaches;
            _authorizationUrlBuilder = authorizationUrlBuilder;
        }

        [HttpGet("{alias}")]
        [ProducesResponseType(typeof(AuthorizedServiceDisplay), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetByAlias(string alias)
        {
            ServiceDetail serviceDetail = ServiceDetailOptions.Get(alias);

            bool isAuthorized = await CheckAuthorizationStatus(serviceDetail);

            string? authorizationUrl = null;
            if (serviceDetail.AuthenticationMethod == AuthenticationMethod.OAuth2AuthorizationCode)
            {
                if (!isAuthorized)
                {
                    AuthorizationPayload authorizationPayload = _authorizationPayloadBuilder.BuildPayload();

                    var cacheKey = CacheHelper.GetPayloadKey(alias);
                    _appCaches.RuntimeCache.Insert(cacheKey, () => authorizationPayload);

                    authorizationUrl = _authorizationUrlBuilder
                        .BuildOAuth2AuthorizationUrl(serviceDetail, HttpContext, authorizationPayload.State, authorizationPayload.CodeChallenge);
                }
            }

            var authorizedServiceDisplay = new AuthorizedServiceDisplay
            {
                Alias = alias,
                DisplayName = serviceDetail.DisplayName,
                IsAuthorized = isAuthorized,
                CanManuallyProvideToken = serviceDetail.CanManuallyProvideToken,
                CanManuallyProvideApiKey = serviceDetail.CanManuallyProvideApiKey,
                AuthorizationUrl = authorizationUrl,
                AuthenticationMethod = serviceDetail.AuthenticationMethod.ToString(),
                SampleRequest = serviceDetail.SampleRequest,
                Settings = new Dictionary<string, string>
                {
                    { nameof(ServiceDetail.Alias), serviceDetail.Alias },
                    { nameof(ServiceDetail.DisplayName), serviceDetail.DisplayName },
                    { nameof(ServiceDetail.ApiHost), serviceDetail.ApiHost },
                    { nameof(ServiceDetail.AuthenticationMethod), serviceDetail.AuthenticationMethod.ToString() },
                    { nameof(ServiceDetail.IdentityHost), serviceDetail.IdentityHost },
                    { nameof(ServiceDetail.TokenHost), serviceDetail.TokenHost },
                    { nameof(ServiceDetail.RequestIdentityPath), serviceDetail.RequestIdentityPath },
                    { nameof(ServiceDetail.RequestTokenPath), serviceDetail.RequestTokenPath },
                    { nameof(ServiceDetail.RequestTokenFormat), serviceDetail.RequestTokenFormat is not null ? serviceDetail.RequestTokenFormat.Value.ToString() : string.Empty },
                    { nameof(ServiceDetail.ApiKey), serviceDetail.ApiKey },
                    { nameof(ServiceDetail.ApiKeyProvision), serviceDetail.ApiKeyProvision is not null ? serviceDetail.ApiKeyProvision.ToString() : string.Empty },
                    { nameof(ServiceDetail.ClientId), serviceDetail.ClientId },
                    { nameof(ServiceDetail.ClientSecret), new string('*', serviceDetail.ClientSecret.Length) },
                    { nameof(ServiceDetail.Scopes), serviceDetail.Scopes },
                    {
                        nameof(ServiceDetail.SampleRequest),
                        !string.IsNullOrEmpty(serviceDetail.SampleRequest)
                            ? serviceDetail.SampleRequest
                            : string.Empty
                    }
                }
            };

            return Ok(authorizedServiceDisplay);
        }
    }
}
