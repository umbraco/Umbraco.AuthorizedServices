using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Umbraco.AuthorizedServices.Api.Management.Controllers.ServiceResponse;
using Umbraco.AuthorizedServices.Configuration;
using Umbraco.AuthorizedServices.Exceptions;
using Umbraco.AuthorizedServices.Models;
using Umbraco.AuthorizedServices.Services;
using Umbraco.Cms.Core.Cache;

namespace Umbraco.AuthorizedServices.Api.Management.Controllers.ServiceResponse
{
    [ApiVersion("1.0")]
    [ApiExplorerSettings(GroupName = Constants.ManagementApi.ServiceResponseGroupName)]
    public class HandleOAuth1IdentityResponseController : AuthorizedServiceResponseControllerBase
    {
        public HandleOAuth1IdentityResponseController(
            IAuthorizedServiceAuthorizer serviceAuthorizer,
            IOptionsMonitor<ServiceDetail> serviceDetailOptions,
            AppCaches appCaches)
            : base(serviceAuthorizer, serviceDetailOptions, appCaches)
        {
        }

        [HttpGet("oauth1")]
        public async Task<IActionResult> Handle(string oauth_token, string oauth_verifier)
        {
            var serviceAlias = AppCaches.RuntimeCache.Get(oauth_token) as string
                ?? throw new AuthorizedServiceException("No cached service with the specified token was found.");

            AuthorizationResult result = await ServiceAuthorizer.AuthorizeOAuth1ServiceAsync(serviceAlias, oauth_token, oauth_verifier);
            if (result.Success)
            {
                return Redirect($"/umbraco#/settings/AuthorizedServices/edit/{serviceAlias}");
            }

            throw new AuthorizedServiceException("Failed to obtain access token");
        }
    }
}
