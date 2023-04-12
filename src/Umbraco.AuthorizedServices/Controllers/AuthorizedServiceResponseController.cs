using Microsoft.AspNetCore.Mvc;
using Umbraco.AuthorizedServices.Exceptions;
using Umbraco.AuthorizedServices.Extensions;
using Umbraco.AuthorizedServices.Models;
using Umbraco.AuthorizedServices.Services;
using Umbraco.AuthorizedServices.Services.Implement;
using Umbraco.Cms.Web.Common.Controllers;

namespace Umbraco.AuthorizedServices.Controllers
{
    /// <summary>
    /// Controller that handles the returning messages for the authorization flow with an external service.
    /// </summary>
    public class AuthorizedServiceResponseController : UmbracoApiController
    {
        private readonly IAuthorizedServiceAuthorizer _serviceAuthorizer;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthorizedServiceResponseController"/> class.
        /// </summary>
        public AuthorizedServiceResponseController(IAuthorizedServiceAuthorizer serviceAuthorizer) => _serviceAuthorizer = serviceAuthorizer;

        /// <summary>
        /// Handles the returning messages for the authorization flow with an external service.
        /// </summary>
        /// <param name="code">The authorization code.</param>
        /// <param name="state">The state.</param>
        public async Task<IActionResult> HandleIdentityResponse(string code, string state)
        {
            var stateParts = state.Split('|');
            if (stateParts.Length != 2)
            {
                throw new AuthorizedServiceException("The state provided in the identity response could not be parsed.");
            }
            
            if (stateParts[1] != StateCache.Instance.Get(stateParts[0])
            {
                throw new AuthorizedServiceException("The state provided in the identity response did not match the expected value.");                
            }

            var serviceAlias = stateParts[0];

            StateCache.Instance.Remove(serviceAlias);

            var redirectUri = HttpContext.GetAuthorizedServiceRedirectUri();
            AuthorizationResult result = await _serviceAuthorizer.AuthorizeServiceAsync(serviceAlias, code, redirectUri);
            if (result.Success)
            {
                return Redirect($"/umbraco#/settings/AuthorizedServices/edit/{serviceAlias}");
            }

            throw new AuthorizedServiceException("Failed to obtain access token");
        }
    }
}
