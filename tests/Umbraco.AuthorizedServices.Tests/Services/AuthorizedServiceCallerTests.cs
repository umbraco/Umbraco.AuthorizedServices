using System.Net;
using System.Net.Http.Headers;
using Azure;
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
        const string ResponseContent = "{ \"foo\": \"bar\" }";
        AuthorizedServiceCaller sut = CreateService(HttpStatusCode.OK, ResponseContent);

        // Act
        Attempt<AuthorizedServiceResponse<TestResponseData>> result = await sut.SendRequestAsync<TestResponseData>(ServiceAlias, path, HttpMethod.Get);

        // Assert
        result.Success.Should().BeTrue();
        result.Result.Should().NotBeNull();

        result.Result!.Data!.Foo.Should().Be("bar");

        result.Result!.RawResponse.Should().Be(ResponseContent);

        result.Result.Metadata.Should().NotBeNull();
        result.Result.Metadata.Date!.Value.ToString("dd MMM yyyy HH:mm:ss").Should().Be("12 Dec 2024 10:59:09");
        result.Result.Metadata.ETag.Should().Be("\"33a64df551425fcc55e4d42a148795d9f25f89d4\"");
        result.Result.Metadata.Expires!.Value.ToString("dd MMM yyyy HH:mm:ss").Should().Be("31 Dec 2024 23:59:59");
        result.Result.Metadata.LastModified!.Value.ToString("dd MMM yyyy HH:mm:ss").Should().Be("01 Dec 2024 23:59:59");
        result.Result.Metadata.Server.Should().Be("github.com");
        result.Result.Metadata.RateLimits.Should().NotBeNull();
        result.Result.Metadata.RateLimits!.Limit = 60;
        result.Result.Metadata.RateLimits.Remaining = 30;

        result.Result.RawHeaders.Count.Should().Be(9);  // We have added 7 explicitly, Content-Type and Content-Length are added automatically.
        result.Result.RawHeaders["Date"].First().Should().Be("Thu, 12 Dec 2024 10:59:09 GMT");
        result.Result.RawHeaders["ETag"].First().Should().Be("\"33a64df551425fcc55e4d42a148795d9f25f89d4\"");
        result.Result.RawHeaders["Expires"].First().Should().Be("Tue, 31 Dec 2024 23:59:59 GMT");
        result.Result.RawHeaders["Last-Modified"].First().Should().Be("Sun, 01 Dec 2024 23:59:59 GMT");
        result.Result.RawHeaders["Server"].First().Should().Be("github.com");
        result.Result.RawHeaders["X-RateLimit-Limit"].First().Should().Be("60");
        result.Result.RawHeaders["X-RateLimit-Remaining"].First().Should().Be("30");

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
        Attempt<AuthorizedServiceResponse<TestResponseData>> result = await sut.SendRequestAsync<TestResponseData>(ServiceAlias, path, HttpMethod.Get);

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
        Attempt<AuthorizedServiceResponse<TestResponseData>> result = await sut.SendRequestAsync<TestResponseData>(ServiceAlias, path, HttpMethod.Get);

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
        Attempt<AuthorizedServiceResponse<string>> result = await sut.SendRequestRawAsync(ServiceAlias, path, HttpMethod.Get);

        // Assert
        result.Success.Should().BeTrue();
        result.Result.Should().NotBeNull();
        result.Result!.Data.Should().Be("{ \"foo\": \"bar\" }");

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
        Attempt<AuthorizedServiceResponse<TestResponseData>> result = await sut.SendRequestAsync<TestRequestData, TestResponseData>(ServiceAlias, path, HttpMethod.Get, data);

        // Assert
        result.Success.Should().BeTrue();
        result.Result.Should().NotBeNull();
        result.Result!.Data!.Foo.Should().Be("bar");

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
        Attempt<AuthorizedServiceResponse<TestResponseData>> result = await sut.SendRequestAsync<TestResponseData>(ServiceAlias, path, HttpMethod.Get);

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
        Attempt<AuthorizedServiceResponse<TestResponseData>> result = await sut.SendRequestAsync<TestResponseData>(ServiceAlias, path, HttpMethod.Get);

        // Assert
        result.Success.Should().BeTrue();
        result.Result.Should().NotBeNull();
        result.Result!.Data!.Foo.Should().Be("bar");

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
        Attempt<AuthorizedServiceResponse<TestResponseData>> result = await sut.SendRequestAsync<TestResponseData>(ServiceAlias, path, HttpMethod.Get);

        // Assert
        result.Success.Should().BeTrue();
        result.Result.Should().NotBeNull();
        result.Result!.Data!.Foo.Should().Be("bar");
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
            authorizedServiceAuthorizerMock.Object,
            new ServiceResponseMetadataParser());
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

            var responseMessage = new HttpResponseMessage
            {
                StatusCode = _statusCode,
                Content = new StringContent(_responseContent ?? string.Empty),
            };
            responseMessage.Headers.Add("Date", "Thu, 12 Dec 2024 10:59:09 GMT");
            responseMessage.Headers.ETag = new EntityTagHeaderValue("\"33a64df551425fcc55e4d42a148795d9f25f89d4\"");
            responseMessage.Content.Headers.Add("Expires", "Tue, 31 Dec 2024 23:59:59 GMT");
            responseMessage.Content.Headers.Add("Last-Modified", "Sun, 01 Dec 2024 23:59:59 GMT");
            responseMessage.Headers.Add("Server", "github.com");
            responseMessage.Headers.Add("X-RateLimit-Limit", "60");
            responseMessage.Headers.Add("X-RateLimit-Remaining", "30");

            httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(responseMessage);
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
