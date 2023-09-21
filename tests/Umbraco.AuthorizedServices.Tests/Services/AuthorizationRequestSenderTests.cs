using Moq.Protected;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Umbraco.AuthorizedServices.Configuration;
using Umbraco.AuthorizedServices.Services;
using Umbraco.AuthorizedServices.Services.Implement;

namespace Umbraco.AuthorizedServices.Tests.Services;

internal class AuthorizationRequestSenderTests
{
    [Test]
    public async Task SendRequest_ForQuerystringProvidedData_SendsRequest()
    {
        // Arrange
        var serviceDetail = new ServiceDetail
        {
            TokenHost = "https://service.url",
            RequestTokenPath = "/login/oauth/access_token",
            RequestTokenFormat = TokenRequestContentFormat.Querystring
        };
        var httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK });
        var clientFactoryMock = new Mock<IAuthorizationClientFactory>();
        clientFactoryMock
            .Setup(x => x.CreateClient())
            .Returns(new HttpClient(httpMessageHandlerMock.Object));
        var parameters = new Dictionary<string, string>
        {
            { "foo", "bar" },
            { "baz", "buzz" }
        };
        var sut = new AuthorizationRequestSender(clientFactoryMock.Object);

        // Act
        await sut.SendRequest(serviceDetail, parameters);

        // Assert
        const string ExpectedUrl = "https://service.url/login/oauth/access_token?foo=bar&baz=buzz";
        httpMessageHandlerMock.Protected()
            .Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(x => x.RequestUri!.ToString() == ExpectedUrl &&
                                                   x.Content == null),
                ItExpr.IsAny<CancellationToken>());
    }

    [Test]
    public async Task SendRequest_FormUrlEncodedProvidedData_SendsRequest()
    {
        // Arrange
        var serviceDetail = new ServiceDetail
        {
            TokenHost = "https://service.url",
            RequestTokenPath = "/login/oauth/access_token",
            RequestTokenFormat = TokenRequestContentFormat.FormUrlEncoded,
        };
        var httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK });
        var clientFactoryMock = new Mock<IAuthorizationClientFactory>();
        clientFactoryMock
            .Setup(x => x.CreateClient())
            .Returns(new HttpClient(httpMessageHandlerMock.Object));
        var parameters = new Dictionary<string, string>
        {
            { "foo", "bar" },
            { "baz", "buzz" }
        };
        var sut = new AuthorizationRequestSender(clientFactoryMock.Object);

        // Act
        await sut.SendRequest(serviceDetail, parameters);

        // Assert
        const string ExpectedUrl = "https://service.url/login/oauth/access_token";
        httpMessageHandlerMock.Protected()
            .Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(x => x.RequestUri!.ToString() == ExpectedUrl &&
                                                   IsExpectedFormUrlContent(x.Content, "foo=bar&baz=buzz")),
                ItExpr.IsAny<CancellationToken>());
    }

    [Test]
    public async Task SendRequest_ForOAuth2ClientCredentialsWithClientCredentialsProvisionInHeader_SendsRequest()
    {
        // Arrange
        var serviceDetail = new ServiceDetail
        {
            TokenHost = "https://service.url",
            ClientId = "TestClientId",
            ClientSecret = "TestClientSecret",
            RequestTokenFormat = TokenRequestContentFormat.FormUrlEncoded,
            AuthenticationMethod = AuthenticationMethod.OAuth2ClientCredentials,
            ClientCredentialsProvision = ClientCredentialsProvision.AuthHeader
        };
        var httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK });
        var clientFactoryMock = new Mock<IAuthorizationClientFactory>();
        clientFactoryMock
            .Setup(x => x.CreateClient())
            .Returns(new HttpClient(httpMessageHandlerMock.Object));
        var parameters = new Dictionary<string, string>
        {
            { "foo", "bar" },
            { "baz", "buzz" }
        };
        var sut = new AuthorizationRequestSender(clientFactoryMock.Object);

        // Act
        await sut.SendRequest(serviceDetail, parameters);

        // Assert
        AuthenticationHeaderValue expectedAuthenticationHeader = BuildBasicAuthenticationHeader(serviceDetail);
        httpMessageHandlerMock.Protected()
            .Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(x => IsAuthenticationHeaderValid(x, expectedAuthenticationHeader)),
                ItExpr.IsAny<CancellationToken>());
    }

    private bool IsExpectedFormUrlContent(HttpContent? content, string expectedContent)
    {
        if (content as FormUrlEncodedContent is null)
        {
            return false;
        }

        var formUrlEncodedContent = (FormUrlEncodedContent)content;
        var contentString = formUrlEncodedContent.ReadAsStringAsync().Result;

        return contentString == expectedContent;
    }

    private bool IsAuthenticationHeaderValid(HttpRequestMessage requestMessage, AuthenticationHeaderValue expectedHeaderValue) =>
        requestMessage.Headers.Authorization is not null
        && requestMessage.Headers.Authorization.Parameter is not null
        && requestMessage.Headers.Authorization.Parameter == expectedHeaderValue.Parameter;

    private AuthenticationHeaderValue BuildBasicAuthenticationHeader(ServiceDetail serviceDetail)
    {
        var authenticationString = $"{serviceDetail.ClientId}:{serviceDetail.ClientSecret}";
        var base64String = Convert.ToBase64String(Encoding.UTF8.GetBytes(authenticationString));

        return new AuthenticationHeaderValue("Basic", base64String);
    }

}
