using Microsoft.AspNetCore.Mvc;
using Umbraco.AuthorizedServices.Extensions;
using Umbraco.AuthorizedServices.Models;
using Umbraco.AuthorizedServices.Services;
using Umbraco.Cms.Web.Common.Controllers;

namespace Umbraco.AuthorizedServices.Controllers
{
    public class AuthorizedServiceResponseController : UmbracoApiController
    {
        private readonly IAuthorizedServiceAuthorizer _serviceAuthorizer;

        public AuthorizedServiceResponseController(IAuthorizedServiceAuthorizer serviceAuthorizer) => _serviceAuthorizer = serviceAuthorizer;

        public async Task<IActionResult> HandleIdentityResponse(string code, string state)
        {
            var stateParts = state.Split('|');
            if (stateParts.Length != 2 && stateParts[1] != Constants.Authorization.State)
            {
                throw new InvalidOperationException("State doesn't match.");
            }

            var serviceAlias = stateParts[0];
            var redirectUri = HttpContext.GetAuthorizedServiceRedirectUri();
            AuthorizationResult result = await _serviceAuthorizer.AuthorizeServiceAsync(serviceAlias, code, redirectUri);
            if (result.Success)
            {
                return Redirect($"/umbraco#/settings/AuthorizedServices/edit/{serviceAlias}");
            }

            throw new InvalidOperationException("Failed to obtain access token");
        }
    }
}
