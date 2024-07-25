using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Umbraco.AuthorizedServices.Configuration;
using Umbraco.AuthorizedServices.Models.Request;
using Umbraco.AuthorizedServices.Services;

namespace Umbraco.AuthorizedServices.Api.Management.Controllers.Service
{
    [ApiVersion("1.0")]
    [ApiExplorerSettings(GroupName = Constants.ManagementApi.ServiceGroupName)]
    public class SaveApiKeyController : AuthorizedServiceStorageController
    {
        public SaveApiKeyController(
            IOptionsMonitor<ServiceDetail> serviceDetailOptions,
            IOAuth2TokenStorage oauth2TokenStorage,
            IOAuth1TokenStorage oauth1TokenStorage,
            IKeyStorage keyStorage)
            : base(serviceDetailOptions, oauth2TokenStorage, oauth1TokenStorage, keyStorage)
        {
        }

        [HttpPost("api-key")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> SaveApiKey(AddApiKey model)
        {
            await KeyStorage.SaveKeyAsync(model.Alias, model.ApiKey);
            return Ok();
        }
    }
}
