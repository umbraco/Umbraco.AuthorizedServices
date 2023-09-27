using Umbraco.AuthorizedServices.Configuration;
using Umbraco.AuthorizedServices.Services.Implement;

namespace Umbraco.AuthorizedServices.Tests.Services;

internal class ExchangeTokenParametersBuilderTests
{
    [Test]
    public void BuildParameters_ReturnsExpectedResult()
    {
        // Arrange
        var serviceDetail = new ServiceDetail
        {
            ClientSecret = "TestClientSecret",
            CanExchangeToken = true,
            ExchangeTokenProvision = new()
            {
                TokenGrantType = "ig_exchange_token"
            }
        };
        const string AccessToken = "abcd";
        var sut = new ExchangeTokenParametersBuilder();

        // Act
        Dictionary<string, string> result = sut.BuildParameters(serviceDetail, AccessToken);

        // Assert
        result.Count.Should().Be(3);
        result["grant_type"].Should().Be("ig_exchange_token");
        result["client_secret"].Should().Be("TestClientSecret");
        result["access_token"].Should().Be(AccessToken);
    }
}
