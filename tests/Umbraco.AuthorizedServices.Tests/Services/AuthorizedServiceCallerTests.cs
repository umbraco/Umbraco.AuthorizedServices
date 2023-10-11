using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq.Protected;
using Umbraco.AuthorizedServices.Configuration;
using Umbraco.AuthorizedServices.Exceptions;
using Umbraco.AuthorizedServices.Models;
using Umbraco.AuthorizedServices.Services;
using Umbraco.AuthorizedServices.Services.Implement;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Infrastructure.Serialization;

namespace Umbraco.AuthorizedServices.Tests.Services;

internal class AuthorizedServiceCallerTests : AuthorizedServiceTestsBase
{
    [SetUp]
    public void SetUp()
    {
        OAuth2TokenStorageMock = new Mock<IOAuth2TokenStorage>();
        OAuth1TokenStorageMock = new Mock<IOAuth1TokenStorage>();
        KeyStorageMock = new Mock<IKeyStorage>();
    }

    [Test]
    public async Task SendRequestAsync_WithoutData_WithValidAccessToken_WithSuccessResponse_ReturnsExpectedResponse()
    {
        // Arrange
        StoreOAuth2Token();

        var path = "/api/test/";
        AuthorizedServiceCaller sut = CreateService(HttpStatusCode.OK, "{ \"foo\": \"bar\" }");

        // Act
        TestResponseData? result = await sut.SendRequestAsync<TestResponseData>(ServiceAlias, path, HttpMethod.Get);

        // Assert
        result.Should().NotBeNull();
        result!.Foo.Should().Be("bar");

        OAuth2TokenStorageMock
            .Verify(x => x.SaveToken(It.IsAny<string>(), It.IsAny<OAuth2Token>()), Times.Never);
    }

    [Test]
    public async Task SendRequestAsync_WithoutData_WithApiKeyFromStorage_WithSuccessResponse_ReturnsExpectedResponse()
    {
        // Arrange
        StoreApiKey();

        var path = "/api/test/";
        AuthorizedServiceCaller sut = CreateService(HttpStatusCode.OK, authenticationMethod: AuthenticationMethod.ApiKey);

        // Act
        TestResponseData? result = await sut.SendRequestAsync<TestResponseData>(ServiceAlias, path, HttpMethod.Get);

        // Assert
        KeyStorageMock
            .Verify(x => x.SaveKey(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task SendRequestAsync_WithoutData_WithValidAccessToken_WithFailedResponse_ThrowsExpectedException()
    {
        // Arrange
        StoreOAuth2Token();

        var path = "/api/test/";
        AuthorizedServiceCaller sut = CreateService(HttpStatusCode.BadRequest);

        // Act
        Func<Task> act = () => sut.SendRequestAsync<TestResponseData>(ServiceAlias, path, HttpMethod.Get);

        // Assert
        await act.Should().ThrowAsync<AuthorizedServiceHttpException>();
    }

    [Test]
    public async Task SendRequestRawAsync_WithoutData_WithValidAccessToken_WithSuccessResponse_ReturnsExpectedResponse()
    {
        // Arrange
        StoreOAuth2Token();

        var path = "/api/test/";
        AuthorizedServiceCaller sut = CreateService(HttpStatusCode.OK, "{ \"foo\": \"bar\" }");

        // Act
        string result = await sut.SendRequestRawAsync(ServiceAlias, path, HttpMethod.Get);

        // Assert
        result.Should().NotBeNull();
        result.Should().Be("{ \"foo\": \"bar\" }");

        OAuth2TokenStorageMock
            .Verify(x => x.SaveToken(It.IsAny<string>(), It.IsAny<OAuth2Token>()), Times.Never);
    }

    [Test]
    public async Task SendRequestAsync_WithData_WithValidAccessToken_WithSuccessReponse_ReturnsExpectedResponse()
    {
        // Arrange
        StoreOAuth2Token();

        var path = "/api/test/";
        var data = new TestRequestData { Baz = "buzz" };
        AuthorizedServiceCaller sut = CreateService(HttpStatusCode.OK, "{ \"foo\": \"bar\" }");

        // Act
        TestResponseData? result = await sut.SendRequestAsync<TestRequestData, TestResponseData>(ServiceAlias, path, HttpMethod.Get, data);

        // Assert
        result.Should().NotBeNull();
        result!.Foo.Should().Be("bar");

        OAuth2TokenStorageMock
            .Verify(x => x.SaveToken(It.IsAny<string>(), It.IsAny<OAuth2Token>()), Times.Never);
    }

    [Test]
    public async Task SendRequestAsync_WithExpiredAccessToken_WithoutRefreshToken_ThrowsExpectedException()
    {
        // Arrange
        var path = "/api/test/";
        AuthorizedServiceCaller sut = CreateService(HttpStatusCode.OK);

        // Act
        Func<Task> act = () => sut.SendRequestAsync<TestResponseData>(ServiceAlias, path, HttpMethod.Get);

        // Assert
        await act.Should().ThrowAsync<AuthorizedServiceException>();
    }

    [Test]
    public async Task SendRequestAsync_WithExpiredAccessToken_WithRefreshTokenFail_ThrowsExpectedException()
    {
        // Arrange
        StoreOAuth2Token(-7);

        var path = "/api/test/";
        AuthorizedServiceCaller sut = CreateService(HttpStatusCode.OK, refreshTokenStatusCode: HttpStatusCode.BadRequest);

        // Act
        Func<Task> act = () => sut.SendRequestAsync<TestResponseData>(ServiceAlias, path, HttpMethod.Get);

        // Assert
        await act.Should().ThrowAsync<AuthorizedServiceException>();
    }

    [Test]
    public async Task SendRequestAsync_WithExpiredAccessToken_WithRefreshTokenSuccess_ReturnsExpectedRespons()
    {
        // Arrange
        StoreOAuth2Token(-7);

        var path = "/api/test/";
        AuthorizedServiceCaller sut = CreateService(HttpStatusCode.OK, "{ \"foo\": \"bar\" }");

        // Act
        TestResponseData? result = await sut.SendRequestAsync<TestResponseData>(ServiceAlias, path, HttpMethod.Get);

        // Assert
        result.Should().NotBeNull();
        result!.Foo.Should().Be("bar");

        OAuth2TokenStorageMock
            .Verify(x => x.SaveToken(It.Is<string>(y => y == ServiceAlias), It.Is<OAuth2Token>(y => y.AccessToken == "abc")), Times.Once);
    }

    [Test]
    public void GetApiKey_WithExistingApiKey_ReturnsApiKey()
    {
        // Arrange
        AuthorizedServiceCaller sut = CreateService(authenticationMethod: AuthenticationMethod.ApiKey, withConfiguredApiKey: true);

        // Act
        var result = sut.GetApiKey(ServiceAlias);

        // Assert
        result.Should().NotBeNull();
        result!.Should().Be("test-api-key");
    }

    [Test]
    public void GetApiKey_WithStoredApiKey_ReturnsStoredApiKey()
    {
        // Arrange
        StoreApiKey();
        AuthorizedServiceCaller sut = CreateService(authenticationMethod: AuthenticationMethod.ApiKey, withConfiguredApiKey: false);

        // Act
        var result = sut.GetApiKey(ServiceAlias);

        // Assert
        result.Should().NotBeNull();
        result!.Should().Be("stored-test-api-key");
    }

    [Test]
    public void GetApiKey_WithoutExistingApiKey_ReturnsEmptyString()
    {
        // Arrange
        AuthorizedServiceCaller sut = CreateService();

        // Act
        var result = sut.GetApiKey(ServiceAlias);

        // Assert
        result.Should().BeNullOrEmpty();
    }

    [Test]
    public void GetOAuth2Token_WithStoredToken_ReturnsAccessToken()
    {
        // Arrange
        StoreOAuth2Token();
        AuthorizedServiceCaller sut = CreateService();

        // Act
        var result = sut.GetOAuth2AccessToken(ServiceAlias);

        // Assert
        result.Should().NotBeNull();
        result!.Should().Be("abc");
    }

    [Test]
    public void GetOAuth2Token_WithoutStoredToken_ReturnsNull()
    {
        // Arrange
        AuthorizedServiceCaller sut = CreateService();

        // Act
        var result = sut.GetOAuth2AccessToken(ServiceAlias);

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public void GetOAuth1Token_WithStoredToken_ReturnsAccessToken()
    {
        // Arrange
        StoreOAuth1Token();
        AuthorizedServiceCaller sut = CreateService();

        // Act
        var result = sut.GetOAuth1Token(ServiceAlias);

        // Assert
        result.Should().NotBeNull();
        result!.Should().Be("abc");
    }

    [Test]
    public void GetOAuth1Token_WithoutStoredToken_ReturnsNull()
    {
        // Arrange
        AuthorizedServiceCaller sut = CreateService();

        // Act
        var result = sut.GetOAuth1Token(ServiceAlias);

        // Assert
        result.Should().BeNull();
    }

    private void StoreOAuth2Token(int daysUntilExpiry = 7) =>
        OAuth2TokenStorageMock
            .Setup(x => x.GetToken(It.Is<string>(y => y == ServiceAlias)))
            .Returns(new OAuth2Token("abc", "def", DateTime.Now.AddDays(daysUntilExpiry)));

    private void StoreOAuth1Token() =>
        OAuth1TokenStorageMock
            .Setup(x => x.GetToken(It.Is<string>(y => y == ServiceAlias)))
            .Returns(new OAuth1Token("abc", "def"));

    private void StoreApiKey() =>
        KeyStorageMock
            .Setup(x => x.GetKey(It.Is<string>(y => y == ServiceAlias)))
            .Returns("stored-test-api-key");


    private AuthorizedServiceCaller CreateService(
        HttpStatusCode statusCode = HttpStatusCode.OK,
        string? responseContent = null,
        HttpStatusCode refreshTokenStatusCode = HttpStatusCode.OK,
        AuthenticationMethod authenticationMethod = AuthenticationMethod.OAuth2AuthorizationCode,
        bool withConfiguredApiKey = false)
    {
        var authorizationRequestSenderMock = new Mock<IAuthorizationRequestSender>();
        var authorizationUrlBuilderMock = new Mock<IAuthorizationUrlBuilder>();
        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();

        // Setup refresh token response.
        var httpResponseMessage = new HttpResponseMessage
        {
            StatusCode = refreshTokenStatusCode,
            Content = new StringContent("{\"refresh_token\": \"def\",\"access_token\": \"abc\",\"expires_in\": 1800}")
        };
        authorizationRequestSenderMock
            .Setup(x => x.SendOAuth2Request(It.Is<ServiceDetail>(y => y.Alias == ServiceAlias), It.Is<Dictionary<string, string>>(y => y["grant_type"] == "refresh_token")))
            .ReturnsAsync(httpResponseMessage);

        Mock<IOptionsMonitor<ServiceDetail>> optionsMonitorServiceDetailMock = CreateOptionsMonitorServiceDetail(authenticationMethod, withConfiguredApiKey);
        var factory = new JsonSerializerFactory(optionsMonitorServiceDetailMock.Object, new JsonNetSerializer());

        return new AuthorizedServiceCaller(
            AppCaches.Disabled,
            new TokenFactory(new DateTimeProvider()),
            OAuth2TokenStorageMock.Object,
            OAuth1TokenStorageMock.Object,
            KeyStorageMock.Object,
            authorizationRequestSenderMock.Object,
            new NullLogger<AuthorizedServiceCaller>(),
            optionsMonitorServiceDetailMock.Object,
            new TestHttpClientFactory(statusCode, responseContent),
            factory,
            authorizationUrlBuilderMock.Object,
            new AuthorizedRequestBuilder(factory),
            new RefreshTokenParametersBuilder(),
            new ExchangeTokenParametersBuilder(),
            httpContextAccessorMock.Object);
    }

    private class TestHttpClientFactory : IHttpClientFactory
    {
        private readonly HttpStatusCode _statusCode;
        private readonly string? _responseContent;

        public TestHttpClientFactory(HttpStatusCode statusCode, string? responseContent)
        {
            _statusCode = statusCode;
            _responseContent = responseContent;
        }

        public HttpClient CreateClient(string name)
        {
            var httpMessageHandlerMock = new Mock<HttpMessageHandler>();
            httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = _statusCode,
                    Content = new StringContent(_responseContent ?? string.Empty),
                });
            return new HttpClient(httpMessageHandlerMock.Object);
        }
    }

    private class TestResponseData
    {
        public string Foo { get; set; } = string.Empty;
    }

    private class TestRequestData
    {
        public string Baz { get; set; } = string.Empty;
    }
}
