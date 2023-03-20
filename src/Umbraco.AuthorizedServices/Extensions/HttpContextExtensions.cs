using Microsoft.AspNetCore.Http;
using Umbraco.AuthorizedServices.Controllers;

namespace Umbraco.AuthorizedServices.Extensions;

internal static class HttpContextExtensions
{
    public static string GetAuthorizedServiceRedirectUri(this HttpContext httpContext)
        => $"{httpContext.Request.Scheme}://{httpContext.Request.Host}/umbraco/api/{nameof(AuthorizedServiceResponseController).Replace("Controller", string.Empty)}/{nameof(AuthorizedServiceResponseController.HandleIdentityResponse)}";
}
