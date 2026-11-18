using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Umbraco.AuthorizedServices.Configuration;
using Umbraco.AuthorizedServices.Controllers;
using Umbraco.AuthorizedServices.Helpers;
using Umbraco.AuthorizedServices.Models;
using Umbraco.AuthorizedServices.Services;
using Umbraco.Cms.Core.Cache;

namespace Umbraco.AuthorizedServices.Tests.Controllers;

internal class AuthorizedServiceResponseControllerTests
{
    private const string ServiceAlias = "testService";
    private const string ExpectedUrl = "/umbraco/section/settings/workspace/authorized-service/edit/testService";

    private Mock<IAuthorizedServiceAuthorizer> _authorizerMock = null!;
    private Mock<IAppPolicyCache> _runtimeCacheMock = null!;
    private Mock<IOptionsMonitor<ServiceDetail>> _optionsMock = null!;

    [SetUp]
    public void SetUp()
    {
        _authorizerMock = new Mock<IAuthorizedServiceAuthorizer>();
        _runtimeCacheMock = new Mock<IAppPolicyCache>();
        _optionsMock = new Mock<IOptionsMonitor<ServiceDetail>>();
    }

    [Test]
    public async Task HandleOAuth1IdentityResponse_WithSuccessfulAuthorization_RedirectsToBackofficeWorkspace()
    {
        // Arrange
        _runtimeCacheMock.Setup(x => x.Get("token")).Returns(ServiceAlias);
        _authorizerMock
            .Setup(x => x.AuthorizeOAuth1ServiceAsync(ServiceAlias, "token", "verifier"))
            .ReturnsAsync(AuthorizationResult.AsSuccess());
        AuthorizedServiceResponseController sut = CreateController();

        // Act
        IActionResult result = await sut.HandleOAuth1IdentityResponse("token", "verifier");

        // Assert
        result.Should().BeOfType<RedirectResult>().Which.Url.Should().Be(ExpectedUrl);
    }

    [Test]
    public async Task HandleOAuth2IdentityResponse_WithTokenExchange_RedirectsToBackofficeWorkspace()
    {
        // Arrange
        const string State = "state";
        var cacheKey = CacheHelper.GetPayloadKey(ServiceAlias);
        _runtimeCacheMock
            .Setup(x => x.Get(cacheKey))
            .Returns(new AuthorizationPayload { State = State, CodeVerifier = "verifier" });
        _authorizerMock
            .Setup(x => x.AuthorizeOAuth2AuthorizationCodeServiceAsync(ServiceAlias, "code", It.IsAny<string>(), "verifier"))
            .ReturnsAsync(AuthorizationResult.AsSuccess());
        _authorizerMock
            .Setup(x => x.ExchangeOAuth2AccessTokenAsync(ServiceAlias))
            .ReturnsAsync(AuthorizationResult.AsSuccess());
        _optionsMock
            .Setup(x => x.Get(ServiceAlias))
            .Returns(new ServiceDetail
            {
                Alias = ServiceAlias,
                CanExchangeToken = true,
                ExchangeTokenProvision = new ExchangeTokenProvision(),
            });
        AuthorizedServiceResponseController sut = CreateController();

        // Act
        IActionResult result = await sut.HandleOAuth2IdentityResponse("code", $"{ServiceAlias}{Constants.Separator}{State}");

        // Assert
        result.Should().BeOfType<RedirectResult>().Which.Url.Should().Be(ExpectedUrl);
    }

    private AuthorizedServiceResponseController CreateController()
    {
        var appCaches = new AppCaches(_runtimeCacheMock.Object, NoAppCache.Instance, new IsolatedCaches(_ => NoAppCache.Instance));
        return new AuthorizedServiceResponseController(_authorizerMock.Object, _optionsMock.Object, appCaches)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() },
        };
    }
}
