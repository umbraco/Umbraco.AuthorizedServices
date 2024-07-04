using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Umbraco.AuthorizedServices.Configuration;
using Umbraco.AuthorizedServices.Models;
using Umbraco.AuthorizedServices.Models.Request;
using Umbraco.AuthorizedServices.Services;

namespace Umbraco.AuthorizedServices.Api.Management.Controllers.Service
{
    [ApiVersion("1.0")]
    [ApiExplorerSettings(GroupName = Constants.ManagementApi.ServiceGroupName)]
    public class SaveOAuth2TokenController : AuthorizedServiceStorageController
    {
        public SaveOAuth2TokenController(
            IOptionsMonitor<ServiceDetail> serviceDetailOptions,
            IOAuth2TokenStorage oauth2TokenStorage,
            IOAuth1TokenStorage oauth1TokenStorage,
            IKeyStorage keyStorage)
            : base(serviceDetailOptions, oauth2TokenStorage, oauth1TokenStorage, keyStorage)
        {
        }

        [HttpPost("oauth2")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> SaveOAuth2Token(AddOAuth2Token model)
        {
            await OAuth2TokenStorage.SaveTokenAsync(model.Alias, new OAuth2Token(model.Token));
            return Ok();
        }
    }
}
