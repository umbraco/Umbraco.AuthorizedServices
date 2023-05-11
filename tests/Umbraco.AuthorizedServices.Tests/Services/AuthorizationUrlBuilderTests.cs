using Microsoft.AspNetCore.Http;
using Umbraco.AuthorizedServices.Configuration;
using Umbraco.AuthorizedServices.Services.Implement;

namespace Umbraco.AuthorizedServices.Tests.Services;

internal class AuthorizationUrlBuilderTests
{
    [Test]
    public void BuildUrl_ReturnsExpectedResult()
    {
        // Arrange
        var serviceDetail = new ServiceDetail
        {
            Alias = "testService",
            IdentityHost = "https://service.url",
            RequestIdentityPath = "/login/oauth/authorize",
            ClientId = "TestClientId",
            Scopes = "test"
        };
        var httpContext = new DefaultHttpContext();
        var sut = new AuthorizationUrlBuilder();
        var state = "state123";
        var codeChallenge = "codeChallenge123";
        // Act
        var result = sut.BuildUrl(serviceDetail, httpContext, state, codeChallenge);

        // Assert
        const string ExpectedUrl =
            "https://service.url/login/oauth/authorize?response_type=code&client_id=TestClientId&response_mode=query&scope=test&state=testService-state123";
        result.Should().Be(ExpectedUrl);
    }

    [Test]
    public void BuildUrl_WithRequiredRedirectUrl_ReturnsExpectedResult()
    {
        // Arrange
        var serviceDetail = new ServiceDetail
        {
            Alias = "testService",
            IdentityHost = "https://service.url",
            RequestIdentityPath = "/login/oauth/authorize",
            ClientId = "TestClientId",
            Scopes = "test",
            AuthorizationUrlRequiresRedirectUrl = true
        };
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Scheme = "https";
        httpContext.Request.Host = new HostString("www.test.com");
        var sut = new AuthorizationUrlBuilder();
        var state = "abc123";
        var codeChallenge = "codeChallenge123";

        // Act
        var result = sut.BuildUrl(serviceDetail, httpContext, state, codeChallenge);

        // Assert
        const string ExpectedUrl =
            "https://service.url/login/oauth/authorize?response_type=code&client_id=TestClientId&response_mode=query&redirect_uri=https://www.test.com/umbraco/api/AuthorizedServiceResponse/HandleIdentityResponse&scope=test&state=testService-abc123";
        result.Should().Be(ExpectedUrl);
    }
}
