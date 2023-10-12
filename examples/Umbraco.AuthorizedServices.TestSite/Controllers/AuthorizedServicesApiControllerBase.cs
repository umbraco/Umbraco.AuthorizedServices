using System.Net;
using Microsoft.AspNetCore.Mvc;
using Umbraco.AuthorizedServices.Exceptions;
using Umbraco.AuthorizedServices.Services;
using Umbraco.Cms.Web.Common.Controllers;

namespace Umbraco.AuthorizedServices.TestSite.Controllers;

public abstract class AuthorizedServicesApiControllerBase : UmbracoApiController
{
    public AuthorizedServicesApiControllerBase(IAuthorizedServiceCaller authorizedServiceCaller) => AuthorizedServiceCaller = authorizedServiceCaller;

    protected IAuthorizedServiceCaller AuthorizedServiceCaller { get; }

    protected IActionResult HandleFailedRequest(Exception? exception, string defaultMessage)
    {
        if (exception is not null)
        {
            if (exception is AuthorizedServiceHttpException authorizedServiceHttpException)
            {
                if (authorizedServiceHttpException.StatusCode == HttpStatusCode.NotFound)
                {
                    return NotFound();
                }

                return StatusCode((int)authorizedServiceHttpException.StatusCode, authorizedServiceHttpException.Reason + ": " + authorizedServiceHttpException.Content);
            }

            if (exception is AuthorizedServiceException authorizedServiceException)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, authorizedServiceException.Message);
            }

            return StatusCode((int)HttpStatusCode.InternalServerError, exception.Message);
        }

        return StatusCode((int)HttpStatusCode.InternalServerError, defaultMessage);
    }
}
