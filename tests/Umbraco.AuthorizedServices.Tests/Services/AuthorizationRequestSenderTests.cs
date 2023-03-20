using Moq.Protected;
using System.Net;
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
            RequestTokenPath = "/login/oauth/access_token"
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

}
