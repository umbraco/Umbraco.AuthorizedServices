using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Umbraco.AuthorizedServices.Api.Management.Controllers.ServiceResponse;
using Umbraco.AuthorizedServices.Configuration;
using Umbraco.AuthorizedServices.Exceptions;
using Umbraco.AuthorizedServices.Extensions;
using Umbraco.AuthorizedServices.Helpers;
using Umbraco.AuthorizedServices.Models;
using Umbraco.AuthorizedServices.Services;
using Umbraco.Cms.Core.Cache;

namespace Umbraco.AuthorizedServices.Api.Management.Controllers.ServiceResponse
{
    [ApiVersion("1.0")]
    [ApiExplorerSettings(GroupName = Constants.ManagementApi.ServiceResponseGroupName)]
    public class HandleOAuth2IdentityResponseController : AuthorizedServiceResponseControllerBase
    {
        public HandleOAuth2IdentityResponseController(
            IAuthorizedServiceAuthorizer serviceAuthorizer,
            IOptionsMonitor<ServiceDetail> serviceDetailOptions,
            AppCaches appCaches)
            : base(serviceAuthorizer, serviceDetailOptions, appCaches)
        {
        }

        [HttpGet("oauth2")]
        public async Task<IActionResult> Handle(string code, string state)
        {
            var stateParts = state.Split(Constants.Separator);
            if (stateParts.Length != 2)
            {
                throw new AuthorizedServiceException("The state provided in the identity response could not be parsed.");
            }

            var cacheKey = CacheHelper.GetPayloadKey(stateParts[0]);
            if (AppCaches.RuntimeCache.Get(cacheKey) is not AuthorizationPayload cachedAuthorizationPayload || stateParts[1] != cachedAuthorizationPayload.State)
            {
                throw new AuthorizedServiceException("The state provided in the identity response did not match the expected value.");
            }

            AppCaches.RuntimeCache.ClearByKey(cacheKey);

            var serviceAlias = stateParts[0];
            var redirectUri = HttpContext.GetOAuth2AuthorizedServiceRedirectUri();
            var codeVerifier = cachedAuthorizationPayload.CodeVerifier;
            AuthorizationResult result = await ServiceAuthorizer.AuthorizeOAuth2AuthorizationCodeServiceAsync(serviceAlias, code, redirectUri, codeVerifier);

            // Handle exchange of short for long-lived token if configured.
            ServiceDetail serviceDetail = ServiceDetailOptions.Get(serviceAlias);
            if (serviceDetail.CanExchangeToken)
            {
                return await HandleTokenExchange(serviceDetail);
            }

            if (result.Success)
            {
                return Redirect($"/umbraco#/settings/AuthorizedServices/edit/{serviceAlias}");
            }

            throw new AuthorizedServiceException("Failed to obtain access token");
        }

        private async Task<IActionResult> HandleTokenExchange(ServiceDetail serviceDetail)
        {
            if (serviceDetail.ExchangeTokenProvision is null)
            {
                throw new AuthorizedServiceException("Failed to retrieve exchange token provisioning.");
            }

            AuthorizationResult exchangeResult = await ServiceAuthorizer.ExchangeOAuth2AccessTokenAsync(serviceDetail.Alias);

            if (exchangeResult.Success)
            {
                return Redirect($"/umbraco#/settings/AuthorizedServices/edit/{serviceDetail.Alias}");
            }

            throw new AuthorizedServiceException("Failed to exchange the access token.");
        }
    }
}
