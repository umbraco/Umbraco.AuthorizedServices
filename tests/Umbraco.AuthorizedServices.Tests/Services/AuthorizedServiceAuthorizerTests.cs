using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Umbraco.AuthorizedServices.Configuration;
using Umbraco.AuthorizedServices.Exceptions;
using Umbraco.AuthorizedServices.Models;
using Umbraco.AuthorizedServices.Services;
using Umbraco.AuthorizedServices.Services.Implement;
using Umbraco.Cms.Core.Cache;

namespace Umbraco.AuthorizedServices.Tests.Services;

internal class AuthorizedServiceAuthorizerTests : AuthorizedServiceTestsBase
{
    [SetUp]
    public void SetUp()
    {
        OAuth2TokenStorageMock = new Mock<IOAuth2TokenStorage>();
        OAuth1TokenStorageMock = new Mock<IOAuth1TokenStorage>();
        KeyStorageMock = new Mock<IKeyStorage>();
    }

    [Test]
    public async Task AuthorizeServiceAsync_WithSuccessResponse_StoresTokenAndReturnsExpectedResponse()
    {
        // Arrange
        AuthorizedServiceAuthorizer sut = CreateService();

        // Act
        AuthorizationResult result = await sut.AuthorizeOAuth2AuthorizationCodeServiceAsync(ServiceAlias, "1234", "https://test.url/handle-auth", "5678");

        // Assert
        result.Success.Should().BeTrue();

        OAuth2TokenStorageMock
            .Verify(x => x.SaveTokenAsync(It.Is<string>(y => y == ServiceAlias), It.Is<OAuth2Token>(y => y.AccessToken == "abc")), Times.Once);
    }

    [Test]
    public async Task AuthorizeServiceAsync_WithFailResponse_ThrowsExpectedException()
    {
        // Arrange
        AuthorizedServiceAuthorizer sut = CreateService(withSuccessReponse: false);

        // Act
        Func<Task> act = () => sut.AuthorizeOAuth2AuthorizationCodeServiceAsync(ServiceAlias, "1234", "https://test.url/handle-auth", "5678");

        // Assert
        await act.Should().ThrowAsync<AuthorizedServiceHttpException>();
    }

    [Test]
    public async Task AuthorizeOAuth1ServiceAsync_WithSuccessResponse_StoresOAuth1TokenAndReturnsExpectedResponse()
    {
        // Arrange
        AuthorizedServiceAuthorizer sut = CreateService(AuthenticationMethod.OAuth1);

        // Act
        AuthorizationResult result = await sut.AuthorizeOAuth1ServiceAsync(ServiceAlias, "1234", "5678");

        // Assert
        result.Success.Should().BeTrue();

        OAuth1TokenStorageMock
            .Verify(x => x.SaveTokenAsync(It.Is<string>(y => y == ServiceAlias), It.Is<OAuth1Token>(y => y.OAuthToken == "abc")), Times.Once);
    }

    [Test]
    public async Task AuthorizeOAuth1ServiceAsync_WithFailResponse_ThrowsExpectedException()
    {
        // Arrange
        AuthorizedServiceAuthorizer sut = CreateService(withSuccessReponse: false);

        // Act
        Func<Task> act = () => sut.AuthorizeOAuth1ServiceAsync(ServiceAlias, "1234", "5678");

        // Assert
        await act.Should().ThrowAsync<AuthorizedServiceHttpException>();
    }

    private AuthorizedServiceAuthorizer CreateService(AuthenticationMethod authenticationMethod = AuthenticationMethod.OAuth2AuthorizationCode, bool withSuccessReponse = true)
    {
        var authorizationRequestSenderMock = new Mock<IAuthorizationRequestSender>();
        HttpResponseMessage httpResponseMessage = withSuccessReponse
            ? new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = authenticationMethod == AuthenticationMethod.OAuth2AuthorizationCode
                    ? new StringContent("{\"refresh_token\": \"def\",\"access_token\": \"abc\",\"expires_in\": 1800}")
                    : new StringContent("oauth_token=abc&oauth_token_secret=def")
            }
            : new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.BadRequest,
            };

        if (authenticationMethod == AuthenticationMethod.OAuth2AuthorizationCode)
        {
            authorizationRequestSenderMock
                .Setup(x => x.SendOAuth2Request(It.Is<ServiceDetail>(y => y.Alias == ServiceAlias), It.IsAny<Dictionary<string, string>>()))
                .ReturnsAsync(httpResponseMessage);
        }
        else if (authenticationMethod == AuthenticationMethod.OAuth1)
        {
            authorizationRequestSenderMock
                .Setup(x => x.SendOAuth1Request(It.Is<ServiceDetail>(y => y.Alias == ServiceAlias), It.IsAny<Dictionary<string, string>>()))
                .ReturnsAsync(httpResponseMessage);
        }

        Mock<IOptionsMonitor<ServiceDetail>> optionsMonitorServiceDetailMock = CreateOptionsMonitorServiceDetail(authenticationMethod);

        return new AuthorizedServiceAuthorizer(
            AppCaches.Disabled,
            new TokenFactory(new DateTimeProvider()),
            OAuth2TokenStorageMock.Object,
            OAuth1TokenStorageMock.Object,
            KeyStorageMock.Object,
            authorizationRequestSenderMock.Object,
            new NullLogger<AuthorizedServiceAuthorizer>(),
            optionsMonitorServiceDetailMock.Object,
            new AuthorizationParametersBuilder(),
            new ExchangeTokenParametersBuilder());
    }
}
