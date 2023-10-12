using System.Net;
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
        TokenStorageMock = new Mock<ITokenStorage>();
        KeyStorageMock = new Mock<IKeyStorage>();
    }

    [Test]
    public async Task SendRequestAsync_WithoutData_WithValidAccessToken_WithSuccessResponse_ReturnsSuccessAttemptWithExpectedResponse()
    {
        // Arrange
        StoreToken();

        var path = "/api/test/";
        AuthorizedServiceCaller sut = CreateService(HttpStatusCode.OK, "{ \"foo\": \"bar\" }");

        // Act
        Attempt<TestResponseData?> result = await sut.SendRequestAsync<TestResponseData>(ServiceAlias, path, HttpMethod.Get);

        // Assert
        result.Success.Should().BeTrue();
        result.Result.Should().NotBeNull();
        result.Result!.Foo.Should().Be("bar");

        TokenStorageMock
            .Verify(x => x.SaveToken(It.IsAny<string>(), It.IsAny<Token>()), Times.Never);
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
            .Verify(x => x.SaveKey(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task SendRequestAsync_WithoutData_WithValidAccessToken_WithFailedResponse_ReturnsFailedAttemptWithExpectedException()
    {
        // Arrange
        StoreToken();

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
        StoreToken();

        var path = "/api/test/";
        AuthorizedServiceCaller sut = CreateService(HttpStatusCode.OK, "{ \"foo\": \"bar\" }");

        // Act
        Attempt<string?> result = await sut.SendRequestRawAsync(ServiceAlias, path, HttpMethod.Get);

        // Assert
        result.Success.Should().BeTrue();
        result.Result.Should().NotBeNull();
        result.Result.Should().Be("{ \"foo\": \"bar\" }");

        TokenStorageMock
            .Verify(x => x.SaveToken(It.IsAny<string>(), It.IsAny<Token>()), Times.Never);
    }

    [Test]
    public async Task SendRequestAsync_WithData_WithValidAccessToken_WithSuccessResponse_ReturnsSuccessAttemptWithExpectedResponse()
    {
        // Arrange
        StoreToken();

        var path = "/api/test/";
        var data = new TestRequestData { Baz = "buzz" };
        AuthorizedServiceCaller sut = CreateService(HttpStatusCode.OK, "{ \"foo\": \"bar\" }");

        // Act
        Attempt<TestResponseData?> result = await sut.SendRequestAsync<TestRequestData, TestResponseData>(ServiceAlias, path, HttpMethod.Get, data);

        // Assert
        result.Success.Should().BeTrue();
        result.Result.Should().NotBeNull();
        result.Result!.Foo.Should().Be("bar");

        TokenStorageMock
            .Verify(x => x.SaveToken(It.IsAny<string>(), It.IsAny<Token>()), Times.Never);
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
        StoreToken(-7);

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
        StoreToken(-7);

        var path = "/api/test/";
        AuthorizedServiceCaller sut = CreateService(HttpStatusCode.OK, "{ \"foo\": \"bar\" }");

        // Act
        Attempt<TestResponseData?> result = await sut.SendRequestAsync<TestResponseData>(ServiceAlias, path, HttpMethod.Get);

        // Assert
        result.Success.Should().BeTrue();
        result.Result.Should().NotBeNull();
        result.Result!.Foo.Should().Be("bar");

        TokenStorageMock
            .Verify(x => x.SaveToken(It.Is<string>(y => y == ServiceAlias), It.Is<Token>(y => y.AccessToken == "abc")), Times.Once);
    }

    [Test]
    public void GetApiKey_WithExistingApiKey_ReturnsSuccessAttemptWithApiKey()
    {
        // Arrange
        AuthorizedServiceCaller sut = CreateService(authenticationMethod: AuthenticationMethod.ApiKey, withConfiguredApiKey: true);

        // Act
        Attempt<string?> result = sut.GetApiKey(ServiceAlias);

        // Assert
        result.Success.Should().BeTrue();
        result.Result.Should().NotBeNull();
        result.Result!.Should().Be("test-api-key");
    }

    [Test]
    public void GetApiKey_WithStoredApiKey_ReturnsSuccessAttemptWithStoredApiKey()
    {
        // Arrange
        StoreApiKey();
        AuthorizedServiceCaller sut = CreateService(authenticationMethod: AuthenticationMethod.ApiKey, withConfiguredApiKey: false);

        // Act
        Attempt<string?> result = sut.GetApiKey(ServiceAlias);

        // Assert
        result.Success.Should().BeTrue();
        result.Result.Should().NotBeNull();
        result.Result!.Should().Be("stored-test-api-key");
    }

    [Test]
    public void GetApiKey_WithoutExistingApiKey_ReturnsFailedAttempt()
    {
        // Arrange
        AuthorizedServiceCaller sut = CreateService();

        // Act
        Attempt<string?> result = sut.GetApiKey(ServiceAlias);

        // Assert
        result.Success.Should().BeFalse();
        result.Result.Should().BeNullOrEmpty();
    }

    [Test]
    public void GetToken_WithStoredToken_ReturnsSuccessAttemptWithAccessToken()
    {
        // Arrange
        StoreToken();
        AuthorizedServiceCaller sut = CreateService();

        // Act
        Attempt<string?> result = sut.GetToken(ServiceAlias);

        // Assert
        result.Success.Should().BeTrue();
        result.Result.Should().NotBeNull();
        result.Result!.Should().Be("abc");
    }

    [Test]
    public void GetToken_WithoutStoredToken_ReturnsFailedAttempt()
    {
        // Arrange
        AuthorizedServiceCaller sut = CreateService();

        // Act
        Attempt<string?> result = sut.GetToken(ServiceAlias);

        // Assert
        result.Success.Should().BeFalse();
    }

    private void StoreToken(int daysUntilExpiry = 7) =>
        TokenStorageMock
            .Setup(x => x.GetToken(It.Is<string>(y => y == ServiceAlias)))
            .Returns(new Token("abc", "def", DateTime.Now.AddDays(daysUntilExpiry)));

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

        // Setup refresh token response.
        var httpResponseMessage = new HttpResponseMessage
        {
            StatusCode = refreshTokenStatusCode,
            Content = new StringContent("{\"refresh_token\": \"def\",\"access_token\": \"abc\",\"expires_in\": 1800}")
        };
        authorizationRequestSenderMock
            .Setup(x => x.SendRequest(It.Is<ServiceDetail>(y => y.Alias == ServiceAlias), It.Is<Dictionary<string, string>>(y => y["grant_type"] == "refresh_token")))
            .ReturnsAsync(httpResponseMessage);

        Mock<IOptionsMonitor<ServiceDetail>> optionsMonitorServiceDetailMock = CreateOptionsMonitorServiceDetail(authenticationMethod, withConfiguredApiKey);
        var factory = new JsonSerializerFactory(optionsMonitorServiceDetailMock.Object, new JsonNetSerializer());

        return new AuthorizedServiceCaller(
            AppCaches.Disabled,
            new TokenFactory(new DateTimeProvider()),
            TokenStorageMock.Object,
            KeyStorageMock.Object,
            authorizationRequestSenderMock.Object,
            new NullLogger<AuthorizedServiceCaller>(),
            optionsMonitorServiceDetailMock.Object,
            new TestHttpClientFactory(statusCode, responseContent),
            factory,
            new AuthorizedRequestBuilder(factory),
            new RefreshTokenParametersBuilder(),
            new ExchangeTokenParametersBuilder());
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
