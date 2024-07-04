using Microsoft.AspNetCore.Http;
using Umbraco.AuthorizedServices.Controllers;

namespace Umbraco.AuthorizedServices.Extensions;

internal static class HttpContextExtensions
{
    public static string GetOAuth2AuthorizedServiceRedirectUri(this HttpContext httpContext)
        => $"{httpContext.Request.Scheme}://{httpContext.Request.Host}/umbraco/api/{nameof(AuthorizedServiceResponseController).Replace("Controller", string.Empty)}/{nameof(AuthorizedServiceResponseController.HandleOAuth2IdentityResponse)}";

    public static string GetOAuth1AuthorizedServiceRedirectUri(this HttpContext httpContext)
        => $"{httpContext.Request.Scheme}://{httpContext.Request.Host}/umbraco/api/{nameof(AuthorizedServiceResponseController).Replace("Controller", string.Empty)}/{nameof(AuthorizedServiceResponseController.HandleOAuth1IdentityResponse)}";
}
