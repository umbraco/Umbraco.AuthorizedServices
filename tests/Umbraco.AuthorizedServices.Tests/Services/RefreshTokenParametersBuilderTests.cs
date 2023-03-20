using Umbraco.AuthorizedServices.Configuration;
using Umbraco.AuthorizedServices.Services.Implement;

namespace Umbraco.AuthorizedServices.Tests.Services;

internal class RefreshTokenParametersBuilderTests
{
    [Test]
    public void BuildParameters_ReturnsExpectedResult()
    {
        // Arrange
        var serviceDetail = new ServiceDetail
        {
            ClientId = "TestClientId",
            ClientSecret = "TestClientSecret"
        };
        const string RefreshToken = "abcd";
        var sut = new RefreshTokenParametersBuilder();

        // Act
        Dictionary<string, string> result = sut.BuildParameters(serviceDetail, RefreshToken);

        // Assert
        result.Count.Should().Be(4);
        result["grant_type"].Should().Be("refresh_token");
        result["client_id"].Should().Be("TestClientId");
        result["client_secret"].Should().Be("TestClientSecret");
        result["refresh_token"].Should().Be(RefreshToken);
    }
}
