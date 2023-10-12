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
    public async Task CreateRequestMessageWithToken_ReturnsExpectedResult()
    {
        // Arrange
        var serviceDetail = new ServiceDetail
        {
            Alias = ServiceAlias,
            ApiHost = "https://service.url",
        };
        const string Path = "/api/test";
        const string AccessToken = "1234";
        var token = new OAuth2Token(AccessToken, null, DateTime.Now.AddDays(7));
        var data = new TestRequestData("bar");
        AuthorizedRequestBuilder sut = CreateSut();

        // Act
        HttpRequestMessage result = sut.CreateRequestMessageWithOAuth2Token(serviceDetail, Path, HttpMethod.Post, token, data);

        // Assert
        var expectedUri = new Uri("https://service.url/api/test");
        await AssertResult(result, expectedUri);

        result.Headers.Count().Should().Be(3);
        result.Headers.Authorization!.ToString().Should().Be("Bearer 1234");
        AssertCommonHeaders(result);
    }

    [Test]
    public async Task CreateRequestMessageWithApiKey_WithKeyProvidedInQueryString_ReturnsExpectedResult()
    {
        // Arrange
        var serviceDetail = new ServiceDetail
        {
            Alias = ServiceAlias,
            ApiHost = "https://service.url",
            ApiKey = "abc",
            ApiKeyProvision = new ApiKeyProvision
            {
                Key = "x-api-key",
                Method = ApiKeyProvisionMethod.QueryString
            }
        };
        const string Path = "/api/test";
        var data = new TestRequestData("bar");
        AuthorizedRequestBuilder sut = CreateSut();

        // Act
        HttpRequestMessage result = sut.CreateRequestMessageWithApiKey(serviceDetail, Path, HttpMethod.Post, "abc", data);

        // Assert
        var expectedUri = new Uri("https://service.url/api/test?x-api-key=abc");
        await AssertResult(result, expectedUri);

        result.Headers.Count().Should().Be(2);
        AssertCommonHeaders(result);
    }

    [Test]
    public async Task CreateRequestMessageWithApiKey_WithKeyProvidedInHttpHeader_ReturnsExpectedResult()
    {
        // Arrange
        var serviceDetail = new ServiceDetail
        {
            Alias = ServiceAlias,
            ApiHost = "https://service.url",
            ApiKey = "abc",
            ApiKeyProvision = new ApiKeyProvision
            {
                Key = "x-api-key",
                Method = ApiKeyProvisionMethod.HttpHeader
            }
        };
        const string Path = "/api/test";
        var data = new TestRequestData("bar");
        AuthorizedRequestBuilder sut = CreateSut();

        // Act
        HttpRequestMessage result = sut.CreateRequestMessageWithApiKey(serviceDetail, Path, HttpMethod.Post, "abc", data);

        // Assert
        var expectedUri = new Uri("https://service.url/api/test");
        await AssertResult(result, expectedUri);

        result.Headers.Count().Should().Be(3);
        result.Headers.Single(x => x.Key == "x-api-key").Value.First().Should().Be("abc");
        AssertCommonHeaders(result);
    }

    [Test]
    public async Task CreateOAuth1RequestWithToken_ReturnsExpectedResult()
    {
        // Arrange
        var serviceDetail = new ServiceDetail
        {
            Alias = ServiceAlias,
            ApiHost = "https://service.url",
            ClientId = "TestClientId",
            ClientSecret = "TestClientSecret"
        };

        const string Path = "/api/test";
        var token = new OAuth1Token("1234", "5678");
        var data = new TestRequestData("bar");
        AuthorizedRequestBuilder sut = CreateSut();

        // Act
        HttpRequestMessage result = sut.CreateRequestMessageWithOAuth1Token(serviceDetail, Path, HttpMethod.Get, token, data);

        // Assert
        result.Method.Should().Be(HttpMethod.Get);
        result.RequestUri.Should().NotBeNull();

        result.RequestUri?.Query.Should().Contain("oauth_consumer_key=");
        result.RequestUri?.Query.Should().Contain("oauth_nonce=");
        result.RequestUri?.Query.Should().Contain("oauth_signature=");
        result.RequestUri?.Query.Should().Contain("oauth_signature_method=HMAC-SHA1");
        result.RequestUri?.Query.Should().Contain("oauth_token=1234");
        result.RequestUri?.Query.Should().Contain("oauth_version=1.0");

        var stringContent = result.Content as StringContent;
        var content = await stringContent!.ReadAsStringAsync();
        content.Should().Be("{\"Foo\":\"bar\"}");
    }

    private static AuthorizedRequestBuilder CreateSut()
    {
        Mock<IOptionsMonitor<ServiceDetail>> optionsMonitorServiceDetailMock = CreateOptionsMonitorServiceDetail();
        var factory = new JsonSerializerFactory(optionsMonitorServiceDetailMock.Object, new JsonNetSerializer());
        return new AuthorizedRequestBuilder(factory);
    }

    private static async Task AssertResult(HttpRequestMessage result, Uri expectedUri)
    {
        result.Method.Should().Be(HttpMethod.Post);
        result.RequestUri.Should().Be(expectedUri);
        result.Content.Should().NotBeNull();
        result.Content.Should().BeOfType<StringContent>();
        var stringContent = result.Content as StringContent;
        var content = await stringContent!.ReadAsStringAsync();
        content.Should().Be("{\"Foo\":\"bar\"}");
    }

    private static void AssertCommonHeaders(HttpRequestMessage result)
    {
        result.Headers.UserAgent.ToString().Should().Be("UmbracoServiceIntegration/1.0.0");
        result.Headers.Accept!.ToString().Should().Be("application/json");
    }

    private class TestRequestData
    {
        public TestRequestData(string foo) => Foo = foo;

        public string Foo { get; }
    }
}
