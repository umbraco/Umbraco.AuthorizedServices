using Umbraco.AuthorizedServices.Configuration;
using Umbraco.AuthorizedServices.Services;
using Umbraco.AuthorizedServices.Services.Implement;

namespace Umbraco.AuthorizedServices.Tests.Services;

internal class TokenFactoryTests
{
    [Test]
    public void CreateFromResponseContent_CreatesOAuth2Token()
    {
        // Arrange
        var serviceDetail = new ServiceDetail
        {
            Alias = "testService",
        };
        const string ResponseContent = "{\"refresh_token\": \"def\",\"access_token\": \"abc\",\"expires_in\": 1800}";
        DateTime utcNow = DateTime.UtcNow;
        var dateTimeProvider = new Mock<IDateTimeProvider>();
        dateTimeProvider
            .Setup(x => x.UtcNow())
            .Returns(utcNow);
        var sut = new TokenFactory(dateTimeProvider.Object);

        // Act
        Models.OAuth2Token result = sut.CreateFromOAuth2ResponseContent(ResponseContent, serviceDetail);

        // Assert
        result.Should().NotBeNull();
        result.AccessToken.Should().Be("abc");
        result.RefreshToken.Should().Be("def");
        (result.ExpiresOn!.Value - utcNow).TotalSeconds.Should().Be(1800);
    }

    [Test]
    public void CreateFromResponseToken_CreatesOAuth1Token()
    {
        // Arrange
        var serviceDetail = new ServiceDetail
        {
            Alias = "testService"
        };

        const string ResponseContent = "oauth_token=token-123&oauth_token_secret=secret-456";
        DateTime utcNow = DateTime.UtcNow;
        var dateTimeProvider = new Mock<IDateTimeProvider>();
        dateTimeProvider
            .Setup(x => x.UtcNow())
            .Returns(utcNow);
        var sut = new TokenFactory(dateTimeProvider.Object);

        // Act
        Models.OAuth1Token result = sut.CreateFromOAuth1ResponseContent(ResponseContent);

        // Assert
        result.Should().NotBeNull();
        result.OAuthToken.Should().Be("token-123");
        result.OAuthTokenSecret.Should().Be("secret-456");
    }

}
