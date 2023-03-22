using Umbraco.AuthorizedServices.Configuration;
using Umbraco.AuthorizedServices.Models;
using Umbraco.AuthorizedServices.Services.Implement;
using Umbraco.Cms.Infrastructure.Serialization;

namespace Umbraco.AuthorizedServices.Tests.Services;

internal class AuthorizedRequestBuilderTests
{
    [Test]
    public async Task CreateRequestMessage_ReturnsExpectedResult()
    {
        // Arrange
        var serviceDetail = new ServiceDetail
        {
            Alias = "testService",
            ApiHost = "https://service.url",
        };
        const string Path = "/api/test";
        const string AccessToken = "1234";
        var token = new Token(AccessToken, null, DateTime.Now.AddDays(7));
        var data = new TestRequestData("bar");
        var serializer = new JsonNetSerializer();
        var sut = new AuthorizedRequestBuilder(serializer);

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

        result.Headers.Count().Should().Be(2);
        result.Headers.Authorization!.ToString().Should().Be("Bearer 1234");
        result.Headers.UserAgent.ToString().Should().Be("UmbracoServiceIntegration/1.0.0");
    }

    private class TestRequestData
    {
        public TestRequestData(string foo) => Foo = foo;

        public string Foo { get; }
    }

}
