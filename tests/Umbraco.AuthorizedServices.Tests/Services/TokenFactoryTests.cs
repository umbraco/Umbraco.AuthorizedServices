using Umbraco.AuthorizedServices.Configuration;
using Umbraco.AuthorizedServices.Services;
using Umbraco.AuthorizedServices.Services.Implement;

namespace Umbraco.AuthorizedServices.Tests.Services;

internal class TokenFactoryTests
{
    [Test]
    public void CreateFromResponseContent_CreatesToken()
    {
        // Arrange
        var serviceDetail = new ServiceDetail
        {
            Alias = "testService",
        };
        const string ResponseContent = "{\r\n  \"refresh_token\": \"def\",\r\n  \"access_token\": \"abc\",\r\n  \"expires_in\": 1800\r\n}";
        DateTime utcNow = DateTime.UtcNow;
        var dateTimeProvider = new Mock<IDateTimeProvider>();
        dateTimeProvider
            .Setup(x => x.UtcNow())
            .Returns(utcNow);
        var sut = new TokenFactory(dateTimeProvider.Object);

        // Act
        Models.Token result = sut.CreateFromResponseContent(ResponseContent, serviceDetail);

        // Assert
        result.Should().NotBeNull();
        result.AccessToken.Should().Be("abc");
        result.RefreshToken.Should().Be("def");
        (result.ExpiresOn!.Value - utcNow).TotalSeconds.Should().Be(1800);
    }
}
