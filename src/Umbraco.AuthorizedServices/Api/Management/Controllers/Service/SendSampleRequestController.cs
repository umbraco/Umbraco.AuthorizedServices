using System.Net;
using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Umbraco.AuthorizedServices.Configuration;
using Umbraco.AuthorizedServices.Exceptions;
using Umbraco.AuthorizedServices.Models;
using Umbraco.AuthorizedServices.Services;
using Umbraco.Cms.Core;

namespace Umbraco.AuthorizedServices.Api.Management.Controllers.Service
{
    [ApiVersion("1.0")]
    [ApiExplorerSettings(GroupName = Constants.ManagementApi.ServiceGroupName)]
    public class SendSampleRequestController : AuthorizedServiceControllerBase
    {
        private readonly IAuthorizedServiceCaller _authorizedServiceCaller;

        public SendSampleRequestController(
            IOptionsMonitor<ServiceDetail> serviceDetailOptions,
            IAuthorizedServiceCaller authorizedServiceCaller)
            : base(serviceDetailOptions)
        {
            _authorizedServiceCaller = authorizedServiceCaller;
        }

        [HttpGet("{alias}/sample-request")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SendSampleRequest(string alias)
        {
            ServiceDetail serviceDetail = ServiceDetailOptions.Get(alias);

            Attempt<AuthorizedServiceResponse<string>> responseAttempt = await _authorizedServiceCaller.SendRequestRawAsync(alias, serviceDetail.SampleRequest ?? string.Empty, HttpMethod.Get);
            if (responseAttempt.Success && responseAttempt.Result is not null)
            {
                return Ok(responseAttempt.Result);
            }

            if (responseAttempt.Exception is not null)
            {
                if (responseAttempt.Exception is AuthorizedServiceHttpException authorizedServiceHttpException)
                {
                    return StatusCode((int)authorizedServiceHttpException.StatusCode, authorizedServiceHttpException.Reason + ": " + authorizedServiceHttpException.Content);
                }

                if (responseAttempt.Exception is AuthorizedServiceException authorizedServiceException)
                {
                    return StatusCode((int)HttpStatusCode.InternalServerError, authorizedServiceException.Message);
                }
            }

            return StatusCode((int)HttpStatusCode.InternalServerError, "Could not complete the sample request due to an unexpected error");
        }
    }
}
