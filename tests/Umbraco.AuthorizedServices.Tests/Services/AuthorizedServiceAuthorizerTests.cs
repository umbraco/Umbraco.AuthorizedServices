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
        TokenStorageMock = new Mock<ITokenStorage>();
    }

    [Test]
    public async Task AuthorizeServiceAsync_WithSuccessResponse_StoresTokenAndReturnsExpectedResponse()
    {
        // Arrange
        AuthorizedServiceAuthorizer sut = CreateService();

        // Act
        AuthorizationResult result = await sut.AuthorizeServiceAsync(ServiceAlias, "1234", "https://test.url/handle-auth");

        // Assert
        result.Success.Should().BeTrue();

        TokenStorageMock
            .Verify(x => x.SaveToken(It.Is<string>(y => y == ServiceAlias), It.Is<Token>(y => y.AccessToken == "abc")), Times.Once);
    }

    [Test]
    public async Task AuthorizeServiceAsync_WithFailResponse_ThrowsExpectedException()
    {
        // Arrange
        AuthorizedServiceAuthorizer sut = CreateService(withSuccessReponse: false);

        // Act
        Func<Task> act = () => sut.AuthorizeServiceAsync(ServiceAlias, "1234", "https://test.url/handle-auth");

        // Assert
        await act.Should().ThrowAsync<AuthorizedServiceHttpException>();
    }

    private AuthorizedServiceAuthorizer CreateService(bool withSuccessReponse = true)
    {
        var authorizationRequestSenderMock = new Mock<IAuthorizationRequestSender>();
        HttpResponseMessage httpResponseMessage = withSuccessReponse
            ? new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = new StringContent("{\"refresh_token\": \"def\",\"access_token\": \"abc\",\"expires_in\": 1800}")
            }
            : new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.BadRequest,
            };
        authorizationRequestSenderMock
            .Setup(x => x.SendRequest(It.Is<ServiceDetail>(y => y.Alias == ServiceAlias), It.IsAny<Dictionary<string, string>>()))
            .ReturnsAsync(httpResponseMessage);

        Mock<IOptionsMonitor<AuthorizedServiceSettings>> optionsMonitorMock = CreateOptionsMonitorSettings();

        return new AuthorizedServiceAuthorizer(
            AppCaches.Disabled,
            new TokenFactory(new DateTimeProvider()),
            TokenStorageMock.Object,
            authorizationRequestSenderMock.Object,
            new NullLogger<AuthorizedServiceAuthorizer>(),
            optionsMonitorMock.Object,
            new AuthorizationParametersBuilder());
    }
}
