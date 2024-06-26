using Microsoft.AspNetCore.Http;

namespace Umbraco.AuthorizedServices.Extensions;

internal static class HttpContextExtensions
{
    public static string GetOAuth2AuthorizedServiceRedirectUri(this HttpContext httpContext) => string.Empty;
    //    => $"{httpContext.Request.Scheme}://{httpContext.Request.Host}/umbraco/api/{nameof(AuthorizedServiceResponseController).Replace("Controller", string.Empty)}/{nameof(AuthorizedServiceResponseController.HandleOAuth2IdentityResponse)}";

    public static string GetOAuth1AuthorizedServiceRedirectUri(this HttpContext httpContext) => string.Empty;
    //    => $"{httpContext.Request.Scheme}://{httpContext.Request.Host}/umbraco/api/{nameof(AuthorizedServiceResponseController).Replace("Controller", string.Empty)}/{nameof(AuthorizedServiceResponseController.HandleOAuth1IdentityResponse)}";
}
