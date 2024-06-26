using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Umbraco.AuthorizedServices.Configuration;
using Umbraco.AuthorizedServices.Exceptions;
using Umbraco.AuthorizedServices.Models;
using Umbraco.AuthorizedServices.Models.Request;
using Umbraco.AuthorizedServices.Services;

namespace Umbraco.AuthorizedServices.Api.Management.Controllers.Service
{
    [ApiVersion("1.0")]
    [ApiExplorerSettings(GroupName = Constants.ManagementApi.ServiceGroupName)]
    public class GenerateOAuth2ClientCredentialsTokenController : AuthorizedServiceControllerBase
    {
        private readonly IAuthorizedServiceAuthorizer _serviceAuthorizer;

        public GenerateOAuth2ClientCredentialsTokenController(
            IOptionsMonitor<ServiceDetail> serviceDetailOptions,
            IAuthorizedServiceAuthorizer serviceAuthorizer)
            : base(serviceDetailOptions) => _serviceAuthorizer = serviceAuthorizer;

        [HttpPost("oauth2/client-credentials")]
        [ProducesResponseType(typeof(AuthorizationResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> GenerateOAuth2ClientCredentialsToken(GenerateToken model)
        {
            ServiceDetail serviceDetail = ServiceDetailOptions.Get(model.Alias);

            AuthorizationResult result = await _serviceAuthorizer
                .AuthorizeOAuth2ClientCredentialsServiceAsync(serviceDetail.Alias);

            if (result.Success)
            {
                return Ok(result);
            }

            throw new AuthorizedServiceException("Failed to obtain access token");
        }
    }
}
