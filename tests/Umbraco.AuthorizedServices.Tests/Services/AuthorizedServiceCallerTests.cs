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
using Umbraco.Cms.Core;
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
    public async Task SendRequestAsync_WithoutData_WithValidAccessToken_WithSuccessResponse_ReturnsSuccessAttemptWithExpectedResponse()
    {
        // Arrange
        StoreOAuth2Token();

        var path = "/api/test/";
        AuthorizedServiceCaller sut = CreateService(HttpStatusCode.OK, "{ \"foo\": \"bar\" }");

        // Act
        Attempt<TestResponseData?> result = await sut.SendRequestAsync<TestResponseData>(ServiceAlias, path, HttpMethod.Get);

        // Assert
        result.Success.Should().BeTrue();
        result.Result.Should().NotBeNull();
        result.Result!.Foo.Should().Be("bar");

        OAuth2TokenStorageMock
            .Verify(x => x.SaveTokenAsync(It.IsAny<string>(), It.IsAny<OAuth2Token>()), Times.Never);
    }

    [Test]
    public async Task SendRequestAsync_WithoutData_WithApiKeyFromStorage_WithSuccessResponse_ReturnsSuccessAttempt()
    {
        // Arrange
        StoreApiKey();

        var path = "/api/test/";
        AuthorizedServiceCaller sut = CreateService(HttpStatusCode.OK, authenticationMethod: AuthenticationMethod.ApiKey);

        // Act
        Attempt<TestResponseData?> result = await sut.SendRequestAsync<TestResponseData>(ServiceAlias, path, HttpMethod.Get);

        // Assert
        result.Success.Should().BeTrue();
        KeyStorageMock
            .Verify(x => x.SaveKeyAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task SendRequestAsync_WithoutData_WithValidAccessToken_WithFailedResponse_ReturnsFailedAttemptWithExpectedException()
    {
        // Arrange
        StoreOAuth2Token();

        var path = "/api/test/";
        AuthorizedServiceCaller sut = CreateService(HttpStatusCode.BadRequest);

        // Act
        Attempt<TestResponseData?> result = await sut.SendRequestAsync<TestResponseData>(ServiceAlias, path, HttpMethod.Get);

        // Assert
        result.Success.Should().BeFalse();
        result.Exception.Should().NotBeNull();
        result.Exception.Should().BeOfType<AuthorizedServiceHttpException>();
    }

    [Test]
    public async Task SendRequestRawAsync_WithoutData_WithValidAccessToken_WithSuccessResponse_ReturnsSuccessAttemptWithExpectedResponse()
    {
        // Arrange
        StoreOAuth2Token();

        var path = "/api/test/";
        AuthorizedServiceCaller sut = CreateService(HttpStatusCode.OK, "{ \"foo\": \"bar\" }");

        // Act
        Attempt<string?> result = await sut.SendRequestRawAsync(ServiceAlias, path, HttpMethod.Get);

        // Assert
        result.Success.Should().BeTrue();
        result.Result.Should().NotBeNull();
        result.Result.Should().Be("{ \"foo\": \"bar\" }");

        OAuth2TokenStorageMock
            .Verify(x => x.SaveTokenAsync(It.IsAny<string>(), It.IsAny<OAuth2Token>()), Times.Never);
    }

    [Test]
    public async Task SendRequestAsync_WithData_WithValidAccessToken_WithSuccessResponse_ReturnsSuccessAttemptWithExpectedResponse()
    {
        // Arrange
        StoreOAuth2Token();

        var path = "/api/test/";
        var data = new TestRequestData { Baz = "buzz" };
        AuthorizedServiceCaller sut = CreateService(HttpStatusCode.OK, "{ \"foo\": \"bar\" }");

        // Act
        Attempt<TestResponseData?> result = await sut.SendRequestAsync<TestRequestData, TestResponseData>(ServiceAlias, path, HttpMethod.Get, data);

        // Assert
        result.Success.Should().BeTrue();
        result.Result.Should().NotBeNull();
        result.Result!.Foo.Should().Be("bar");

        OAuth2TokenStorageMock
            .Verify(x => x.SaveTokenAsync(It.IsAny<string>(), It.IsAny<OAuth2Token>()), Times.Never);
    }

    [Test]
    public async Task SendRequestAsync_WithExpiredAccessToken_WithoutRefreshToken_ReturnsFailedAttemptWithExpectedException()
    {
        // Arrange
        var path = "/api/test/";
        AuthorizedServiceCaller sut = CreateService(HttpStatusCode.OK);

        // Act
        Attempt<TestResponseData?> result = await sut.SendRequestAsync<TestResponseData>(ServiceAlias, path, HttpMethod.Get);

        // Assert
        result.Success.Should().BeFalse();
        result.Exception.Should().NotBeNull();
        result.Exception.Should().BeOfType<AuthorizedServiceException>();
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
    public async Task SendRequestAsync_WithExpiredAccessToken_WithRefreshTokenSuccess_ReturnsSuccessAttemptWithExpectedResponse()
    {
        // Arrange
        StoreOAuth2Token(-7);

        var path = "/api/test/";
        AuthorizedServiceCaller sut = CreateService(HttpStatusCode.OK, "{ \"foo\": \"bar\" }");

        // Act
        Attempt<TestResponseData?> result = await sut.SendRequestAsync<TestResponseData>(ServiceAlias, path, HttpMethod.Get);

        // Assert
        result.Success.Should().BeTrue();
        result.Result.Should().NotBeNull();
        result.Result!.Foo.Should().Be("bar");

        OAuth2TokenStorageMock
            .Verify(x => x.SaveTokenAsync(It.Is<string>(y => y == ServiceAlias), It.Is<OAuth2Token>(y => y.AccessToken == "abc")), Times.Once);
    }

    [Test]
    public async Task SendRequestAsync_WithExpiredAccessToken_WithClientCredentialsSuccess_ReturnsSuccessAttemptWithExpectedResponse()
    {
        // Arrange
        StoreOAuth2Token(-7);

        var path = "/api/test/";
        AuthorizedServiceCaller sut = CreateService(HttpStatusCode.OK, "{ \"foo\": \"bar\" }", authenticationMethod: AuthenticationMethod.OAuth2ClientCredentials);

        // Act
        Attempt<TestResponseData?> result = await sut.SendRequestAsync<TestResponseData>(ServiceAlias, path, HttpMethod.Get);

        // Assert
        result.Success.Should().BeTrue();
        result.Result.Should().NotBeNull();
        result.Result!.Foo.Should().Be("bar");
    }

    [Test]
    public async Task GetApiKey_WithExistingApiKey_ReturnsSuccessAttemptWithApiKey()
    {
        // Arrange
        AuthorizedServiceCaller sut = CreateService(authenticationMethod: AuthenticationMethod.ApiKey, withConfiguredApiKey: true);

        // Act
        Attempt<string?> result = await sut.GetApiKey(ServiceAlias);

        // Assert
        result.Success.Should().BeTrue();
        result.Result.Should().NotBeNull();
        result.Result!.Should().Be("test-api-key");
    }

    [Test]
    public async Task GetApiKey_WithStoredApiKey_ReturnsSuccessAttemptWithStoredApiKey()
    {
        // Arrange
        StoreApiKey();
        AuthorizedServiceCaller sut = CreateService(authenticationMethod: AuthenticationMethod.ApiKey, withConfiguredApiKey: false);

        // Act
        Attempt<string?> result = await sut.GetApiKey(ServiceAlias);

        // Assert
        result.Success.Should().BeTrue();
        result.Result.Should().NotBeNull();
        result.Result!.Should().Be("stored-test-api-key");
    }

    [Test]
    public async Task GetApiKey_WithoutExistingApiKey_ReturnsFailedAttempt()
    {
        // Arrange
        AuthorizedServiceCaller sut = CreateService();

        // Act
        Attempt<string?> result = await sut.GetApiKey(ServiceAlias);

        // Assert
        result.Success.Should().BeFalse();
        result.Result.Should().BeNullOrEmpty();
    }

    [Test]
    public async Task GetOAuth2Token_WithStoredToken_ReturnsSuccessAttemptWithAccessToken()
    {
        // Arrange
        StoreOAuth2Token();
        AuthorizedServiceCaller sut = CreateService();

        // Act
        Attempt<string?> result = await sut.GetOAuth2AccessToken(ServiceAlias);

        // Assert
        result.Success.Should().BeTrue();
        result.Result.Should().NotBeNull();
        result.Result!.Should().Be("abc");
    }

    [Test]
    public async Task GetOAuth2AccessToken_WithoutStoredToken_ReturnsFailedAttempt()
    {
        // Arrange
        AuthorizedServiceCaller sut = CreateService();

        // Act
        Attempt<string?> result = await sut.GetOAuth2AccessToken(ServiceAlias);

        // Assert
        result.Success.Should().BeFalse();
    }

    [Test]
    public async Task GetOAuth1Token_WithStoredToken_ReturnsSuccessAttemptWithAccessToken()
    {
        // Arrange
        StoreOAuth1Token();
        AuthorizedServiceCaller sut = CreateService();

        // Act
        Attempt<string?> result = await sut.GetOAuth1Token(ServiceAlias);

        // Assert
        result.Success.Should().BeTrue();
        result.Result.Should().NotBeNull();
        result.Result!.Should().Be("abc");
    }

    [Test]
    public async Task GetOAuth1Token_WithoutStoredToken_ReturnsFailedAttempt()
    {
        // Arrange
        AuthorizedServiceCaller sut = CreateService();

        // Act
        Attempt<string?> result = await sut.GetOAuth1Token(ServiceAlias);

        // Assert
        result.Success.Should().BeFalse();
    }

    private void StoreOAuth2Token(int daysUntilExpiry = 7) =>
        OAuth2TokenStorageMock
            .Setup(x => x.GetTokenAsync(It.Is<string>(y => y == ServiceAlias)))
            .ReturnsAsync(new OAuth2Token("abc", "def", DateTime.Now.AddDays(daysUntilExpiry)));

    private void StoreOAuth1Token() =>
        OAuth1TokenStorageMock
            .Setup(x => x.GetTokenAsync(It.Is<string>(y => y == ServiceAlias)))
            .ReturnsAsync(new OAuth1Token("abc", "def"));

    private void StoreApiKey() =>
        KeyStorageMock
            .Setup(x => x.GetKeyAsync(It.Is<string>(y => y == ServiceAlias)))
            .ReturnsAsync("stored-test-api-key");


    private AuthorizedServiceCaller CreateService(
        HttpStatusCode statusCode = HttpStatusCode.OK,
        string? responseContent = null,
        HttpStatusCode refreshTokenStatusCode = HttpStatusCode.OK,
        AuthenticationMethod authenticationMethod = AuthenticationMethod.OAuth2AuthorizationCode,
        bool withConfiguredApiKey = false)
    {
        var authorizationRequestSenderMock = new Mock<IAuthorizationRequestSender>();
        var authorizedServiceAuthorizerMock = new Mock<IAuthorizedServiceAuthorizer>();

        // Setup refresh token response.
        var httpResponseMessage = new HttpResponseMessage
        {
            StatusCode = refreshTokenStatusCode,
            Content = new StringContent("{\"refresh_token\": \"def\",\"access_token\": \"abc\",\"expires_in\": 1800}")
        };
        authorizationRequestSenderMock
            .Setup(x => x.SendOAuth2Request(It.Is<ServiceDetail>(y => y.Alias == ServiceAlias), It.Is<Dictionary<string, string>>(y => y["grant_type"] == "refresh_token")))
            .ReturnsAsync(httpResponseMessage);

        // Setup client credentials token response.
        authorizedServiceAuthorizerMock
            .Setup(x => x.AuthorizeOAuth2ClientCredentialsServiceAsync(It.Is<string>(y => y == ServiceAlias)))
            .ReturnsAsync(AuthorizationResult.AsSuccess());

        Mock<IOptionsMonitor<ServiceDetail>> optionsMonitorServiceDetailMock = CreateOptionsMonitorServiceDetail(authenticationMethod, withConfiguredApiKey);

        var jsonSerializer = new SystemTextJsonSerializer();
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
            jsonSerializer,
            new AuthorizedRequestBuilder(jsonSerializer),
            new RefreshTokenParametersBuilder(),
            new ExchangeTokenParametersBuilder(),
            authorizedServiceAuthorizerMock.Object);
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
