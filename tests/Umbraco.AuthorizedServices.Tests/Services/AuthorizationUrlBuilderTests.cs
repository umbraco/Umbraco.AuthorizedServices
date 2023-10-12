using Microsoft.AspNetCore.Http;
using Umbraco.AuthorizedServices.Configuration;
using Umbraco.AuthorizedServices.Extensions;
using Umbraco.AuthorizedServices.Helpers;
using Umbraco.AuthorizedServices.Services.Implement;

namespace Umbraco.AuthorizedServices.Tests.Services;

internal class AuthorizationUrlBuilderTests
{
    [Test]
    public void BuildUrl_ReturnsExpectedResult()
    {
        // Arrange
        var serviceDetail = new ServiceDetail
        {
            Alias = "testService",
            IdentityHost = "https://service.url",
            RequestIdentityPath = "/login/oauth/authorize",
            ClientId = "TestClientId",
            Scopes = "test"
        };
        var httpContext = new DefaultHttpContext();
        var sut = new AuthorizationUrlBuilder();
        var state = "state123";
        var codeChallenge = "codeChallenge123";
        // Act
        var result = sut.BuildOAuth2AuthorizationUrl(serviceDetail, httpContext, state, codeChallenge);

        // Assert
        const string ExpectedUrl =
            "https://service.url/login/oauth/authorize?response_type=code&client_id=TestClientId&response_mode=query&scope=test&state=testService-state123";
        result.Should().Be(ExpectedUrl);
    }

    [Test]
    public void BuildUrl_WithRequiredRedirectUrl_ReturnsExpectedResult()
    {
        // Arrange
        var serviceDetail = new ServiceDetail
        {
            Alias = "testService",
            IdentityHost = "https://service.url",
            RequestIdentityPath = "/login/oauth/authorize",
            ClientId = "TestClientId",
            Scopes = "test",
            AuthorizationUrlRequiresRedirectUrl = true
        };
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Scheme = "https";
        httpContext.Request.Host = new HostString("www.test.com");
        var sut = new AuthorizationUrlBuilder();
        var state = "abc123";
        var codeChallenge = "codeChallenge123";

        // Act
        var result = sut.BuildOAuth2AuthorizationUrl(serviceDetail, httpContext, state, codeChallenge);

        // Assert
        const string ExpectedUrl =
            "https://service.url/login/oauth/authorize?response_type=code&client_id=TestClientId&response_mode=query&redirect_uri=https://www.test.com/umbraco/api/AuthorizedServiceResponse/HandleOAuth2IdentityResponse&scope=test&state=testService-abc123";
        result.Should().Be(ExpectedUrl);
    }

    [Test]
    public void BuildOAuth1Url_WithNoRedirectUrl_ReturnsExpectedResult()
    {
        // Arrange
        var serviceDetail = new ServiceDetail
        {
            Alias = "testService",
            IdentityHost = "https://service.url",
            RequestAuthorizationPath = "/oauth/authorize",
            ClientId = "TestClientId",
            ClientSecret = "TestClientSecret"
        };

        string nonce = OAuth1Helper.GetNonce();
        string timestamp = OAuth1Helper.GetTimestamp();

        var authorizationParameters = new Dictionary<string, string>
        {
            { "oauth_consumer_key", "TestClientId" },
            { "oauth_nonce", nonce },
            { "oauth_signature_method", "HMAC-SHA1" },
            { "oauth_timestamp", timestamp },
            { "oauth_version", "1.0" }
        };

        var httpContext = new DefaultHttpContext();

        HttpMethod httpMethod = HttpMethod.Get;

        var signature = OAuth1Helper.GetSignature(
            httpMethod.ToString(),
            $"{serviceDetail.IdentityHost}{serviceDetail.RequestAuthorizationPath}",
            serviceDetail.ClientSecret,
            null,
            authorizationParameters);

        var sut = new AuthorizationUrlBuilder();

        // Act
        var result = sut.BuildOAuth1RequestTokenUrl(serviceDetail, httpContext, httpMethod, nonce, timestamp);

        // Assert
        string expectedUrl = "https://service.url/oauth/authorize"
            + "?oauth_consumer_key=TestClientId"
            + "&oauth_nonce=" + nonce
            + "&oauth_signature=" + Uri.EscapeDataString(signature)
            + "&oauth_signature_method=HMAC-SHA1"
            + "&oauth_timestamp=" + timestamp
            + "&oauth_version=1.0";
        result.Should().Be(expectedUrl);
    }

    [Test]
    public void BuildOAuth1Url_WithRequiredRedirectUrl_ReturnsExpectedResult()
    {
        // Arrange
        var serviceDetail = new ServiceDetail
        {
            Alias = "testService",
            IdentityHost = "https://service.url",
            RequestAuthorizationPath = "/oauth/authorize",
            ClientId = "TestClientId",
            ClientSecret = "TestClientSecret",
            AuthorizationUrlRequiresRedirectUrl = true
        };

        var httpContext = new DefaultHttpContext();

        string nonce = OAuth1Helper.GetNonce();
        string timestamp = OAuth1Helper.GetTimestamp();
        string redirectUrl = httpContext.GetOAuth1AuthorizedServiceRedirectUri();

        var authorizationParameters = new Dictionary<string, string>
        {
            { "oauth_callback", redirectUrl },
            { "oauth_consumer_key", "TestClientId" },
            { "oauth_nonce", nonce },
            { "oauth_signature_method", "HMAC-SHA1" },
            { "oauth_timestamp", timestamp },
            { "oauth_version", "1.0" }
        };

        HttpMethod httpMethod = HttpMethod.Get;

        var signature = OAuth1Helper.GetSignature(
            httpMethod.ToString(),
            $"{serviceDetail.IdentityHost}{serviceDetail.RequestAuthorizationPath}",
            serviceDetail.ClientSecret,
            null,
            authorizationParameters);

        var sut = new AuthorizationUrlBuilder();

        // Act
        var result = sut.BuildOAuth1RequestTokenUrl(serviceDetail, httpContext, httpMethod, nonce, timestamp);

        // Assert
        string expectedUrl = "https://service.url/oauth/authorize"
            + "?oauth_callback=" + Uri.EscapeDataString(redirectUrl)
            + "&oauth_consumer_key=TestClientId"
            + "&oauth_nonce=" + nonce
            + "&oauth_signature=" + Uri.EscapeDataString(signature)
            + "&oauth_signature_method=HMAC-SHA1"
            + "&oauth_timestamp=" + timestamp
            + "&oauth_version=1.0";
        result.Should().Be(expectedUrl);
    }
}
