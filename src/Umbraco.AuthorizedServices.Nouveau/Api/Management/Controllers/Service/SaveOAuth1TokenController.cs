using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Umbraco.AuthorizedServices.Api.Management.Controllers.Service;
using Umbraco.AuthorizedServices.Configuration;
using Umbraco.AuthorizedServices.Models;
using Umbraco.AuthorizedServices.Models.Request;
using Umbraco.AuthorizedServices.Services;

namespace Umbraco.AuthorizedServices.Api.Management.Controllers.Service
{
    [ApiVersion("1.0")]
    [ApiExplorerSettings(GroupName = Constants.ManagementApi.ServiceGroupName)]
    public class SaveOAuth1TokenController : AuthorizedServiceStorageController
    {
        public SaveOAuth1TokenController(IOptionsMonitor<ServiceDetail> serviceDetailOptions, IOAuth2TokenStorage oauth2TokenStorage, IOAuth1TokenStorage oauth1TokenStorage, IKeyStorage keyStorage) : base(serviceDetailOptions, oauth2TokenStorage, oauth1TokenStorage, keyStorage)
        {
        }

        [HttpPost("oauth1")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> SaveOAuth1Token(AddOAuth1Token model)
        {
            await OAuth1TokenStorage.SaveTokenAsync(model.Alias, new OAuth1Token(model.Token, model.TokenSecret));
            return Ok();
        }
    }
}
