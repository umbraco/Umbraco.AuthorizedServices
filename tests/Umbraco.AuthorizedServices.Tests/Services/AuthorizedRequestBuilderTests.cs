using Microsoft.Extensions.Options;
using Umbraco.AuthorizedServices.Configuration;
using Umbraco.AuthorizedServices.Models;
using Umbraco.AuthorizedServices.Services;
using Umbraco.AuthorizedServices.Services.Implement;
using Umbraco.Cms.Infrastructure.Serialization;

namespace Umbraco.AuthorizedServices.Tests.Services;

internal class AuthorizedRequestBuilderTests : AuthorizedServiceTestsBase
{
    [Test]
    public async Task CreateRequestMessage_ReturnsExpectedResult()
    {
        // Arrange
        var serviceDetail = new ServiceDetail
        {
            Alias = ServiceAlias,
            ApiHost = "https://service.url",
        };
        const string Path = "/api/test";
        const string AccessToken = "1234";
        var token = new Token(AccessToken, null, DateTime.Now.AddDays(7));
        var data = new TestRequestData("bar");
        Mock<IOptionsMonitor<ServiceDetail>> optionsMonitorServiceDetailMock = CreateOptionsMonitorServiceDetail();
        var factory = new JsonSerializerFactory(optionsMonitorServiceDetailMock.Object, new JsonNetSerializer());
        var sut = new AuthorizedRequestBuilder(factory);

        // Act
        HttpRequestMessage result = sut.CreateRequestMessage(serviceDetail, Path, HttpMethod.Post, token, data);

        // Assert
        var expectedUri = new Uri("https://service.url/api/test");
        result.Method.Should().Be(HttpMethod.Post);
        result.RequestUri.Should().Be(expectedUri);
        result.Content.Should().NotBeNull();
        result.Content.Should().BeOfType<StringContent>();
        var stringContent = result.Content as StringContent;
        var content = await stringContent!.ReadAsStringAsync();
        content.Should().Be("{\"Foo\":\"bar\"}");

        result.Headers.Count().Should().Be(3);
        result.Headers.Authorization!.ToString().Should().Be("Bearer 1234");
        result.Headers.UserAgent.ToString().Should().Be("UmbracoServiceIntegration/1.0.0");
        result.Headers.Accept!.ToString().Should().Be("application/json");
    }

    private class TestRequestData
    {
        public TestRequestData(string foo) => Foo = foo;

        public string Foo { get; }
    }
}
