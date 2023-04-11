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
            Scopes = "test",
            State = "state123"
        };
        var httpContext = new DefaultHttpContext();
        var sut = new AuthorizationUrlBuilder();

        // Act
        var result = sut.BuildUrl(serviceDetail, httpContext);

        // Assert
        const string ExpectedUrl =
            "https://service.url/login/oauth/authorize?client_id=TestClientId&scope=test&response_type=code&response_mode=query&state=testService|state123";
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
            AuthorizationRequestsRequireRedirectUri = true,
            State = "state123"
        };
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Scheme = "https";
        httpContext.Request.Host = new HostString("www.test.com");
        var sut = new AuthorizationUrlBuilder();

        // Act
        var result = sut.BuildUrl(serviceDetail, httpContext);

        // Assert
        const string ExpectedUrl =
            "https://service.url/login/oauth/authorize?client_id=TestClientId&redirect_uri=https://www.test.com/umbraco/api/AuthorizedServiceResponse/HandleIdentityResponse&scope=test&response_type=code&response_mode=query&state=testService|abc123";
        result.Should().Be(ExpectedUrl);
    }
}
