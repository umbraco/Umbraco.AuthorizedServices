using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.AuthorizedServices.Controllers;
using AuthorizedServicesHttpContextExtensions = Umbraco.AuthorizedServices.Extensions.HttpContextExtensions;

namespace Umbraco.AuthorizedServices.Tests.Controllers;

internal class AuthorizedServiceResponseControllerRoutingTests
{
    [TestCase(nameof(AuthorizedServiceResponseController.HandleOAuth1IdentityResponse))]
    [TestCase(nameof(AuthorizedServiceResponseController.HandleOAuth2IdentityResponse))]
    public void CallbackAction_IsRoutedAtItsOwnName(string actionName)
    {
        MethodInfo action = typeof(AuthorizedServiceResponseController).GetMethod(actionName)!;

        action.GetCustomAttributes<HttpGetAttribute>().Select(x => x.Template)
            .Should().Contain(actionName);
    }

    [Test]
    public void OAuth1RedirectUri_MatchesTheRouteOfTheOAuth1Action()
        => AssertRedirectUriMatchesRoute(
            AuthorizedServicesHttpContextExtensions.GetOAuth1AuthorizedServiceRedirectUri,
            nameof(AuthorizedServiceResponseController.HandleOAuth1IdentityResponse));

    [Test]
    public void OAuth2RedirectUri_MatchesTheRouteOfTheOAuth2Action()
        => AssertRedirectUriMatchesRoute(
            AuthorizedServicesHttpContextExtensions.GetOAuth2AuthorizedServiceRedirectUri,
            nameof(AuthorizedServiceResponseController.HandleOAuth2IdentityResponse));

    private static void AssertRedirectUriMatchesRoute(Func<HttpContext, string> getRedirectUri, string actionName)
    {
        var context = new DefaultHttpContext();
        context.Request.Scheme = "https";
        context.Request.Host = new HostString("example.com");

        var classRoute = typeof(AuthorizedServiceResponseController).GetCustomAttribute<RouteAttribute>()!.Template;
        var actionRoute = typeof(AuthorizedServiceResponseController).GetMethod(actionName)!
            .GetCustomAttributes<HttpGetAttribute>().Select(x => x.Template).Single(x => x == actionName);

        getRedirectUri(context).Should().Be($"https://example.com{classRoute}/{actionRoute}");
    }
}
