using Umbraco.AuthorizedServices.Configuration;
using Umbraco.AuthorizedServices.Services.Implement;

namespace Umbraco.AuthorizedServices.Tests.Services;

internal class AuthorizationParametersBuilderTests
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
        const string AuthorizationCode = "1234";
        const string RedirectUrl = "https://test.url";
        var sut = new AuthorizationParametersBuilder();

        // Act
        Dictionary<string, string> result = sut.BuildParameters(serviceDetail, AuthorizationCode, RedirectUrl);

        // Assert
        result.Count.Should().Be(5);
        result["grant_type"].Should().Be("authorization_code");
        result["client_id"].Should().Be("TestClientId");
        result["client_secret"].Should().Be("TestClientSecret");
        result["code"].Should().Be(AuthorizationCode);
        result["redirect_uri"].Should().Be(RedirectUrl);
    }
}
