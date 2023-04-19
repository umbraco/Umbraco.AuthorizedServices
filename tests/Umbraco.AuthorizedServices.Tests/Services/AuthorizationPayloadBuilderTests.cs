using Umbraco.AuthorizedServices.Services.Implement;

namespace Umbraco.AuthorizedServices.Tests.Services;

internal class AuthorizationPayloadBuilderTests
{
    [Test]
    public void BuildPayload_ReturnsValidResult()
    {
        // Arrange
        var sut = new AuthorizationPayloadBuilder();

        // Act
        var result = sut.BuildPayload();

        // Assert
        Assert.NotNull(result);
        Assert.IsNotEmpty(result.State);
        Assert.IsNotEmpty(result.CodeVerifier);
        Assert.IsNotEmpty(result.CodeChallenge);
    }
}
