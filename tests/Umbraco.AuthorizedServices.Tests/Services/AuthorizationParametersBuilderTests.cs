using Umbraco.AuthorizedServices.Configuration;
using Umbraco.AuthorizedServices.Services.Implement;

namespace Umbraco.AuthorizedServices.Tests.Services;

internal class AuthorizationParametersBuilderTests
{
    private readonly ServiceDetail _serviceDetail = new ServiceDetail();

    [SetUp]
    public void Setup()
    {
        _serviceDetail.ClientId = "TestClientId";
        _serviceDetail.ClientSecret = "TestClientSecret";
    }


    [Test]
    public void BuildParameters_ForOAuth2AuthorizationCode_ReturnsExpectedResult()
    {
        // Arrange
        const string AuthorizationCode = "1234";
        const string RedirectUrl = "https://test.url";
        const string CodeVerifier = "TestCodeVerifier";
        var sut = new AuthorizationParametersBuilder();

        // Act
        Dictionary<string, string> result = sut.BuildParametersForOAuth2AuthorizationCode(_serviceDetail, AuthorizationCode, RedirectUrl, CodeVerifier);

        // Assert
        result.Count.Should().Be(5);
        result["grant_type"].Should().Be("authorization_code");
        result["client_id"].Should().Be("TestClientId");
        result["client_secret"].Should().Be("TestClientSecret");
        result["code"].Should().Be(AuthorizationCode);
        result["redirect_uri"].Should().Be(RedirectUrl);
    }

    [Test]
    public void BuildParameters_ForOAuth2AuthorizationCode_WithCodeVerifier_ReturnsExpectedResult()
    {
        // Arrange
        _serviceDetail.UseProofKeyForCodeExchange = true;

        const string AuthorizationCode = "1234";
        const string RedirectUrl = "https://test.url";
        const string CodeVerifier = "TestCodeVerifier";
        var sut = new AuthorizationParametersBuilder();

        // Act
        Dictionary<string, string> result = sut.BuildParametersForOAuth2AuthorizationCode(_serviceDetail, AuthorizationCode, RedirectUrl, CodeVerifier);

        // Assert
        result.Count.Should().Be(6);
        result["grant_type"].Should().Be("authorization_code");
        result["client_id"].Should().Be("TestClientId");
        result["client_secret"].Should().Be("TestClientSecret");
        result["code"].Should().Be(AuthorizationCode);
        result["redirect_uri"].Should().Be(RedirectUrl);
        result["code_verifier"].Should().Be(CodeVerifier);
    }

    [Test]
    public void BuildParameters_ForOAuth2ClientCredentials_WithScopes_ReturnsExpectedResult()
    {
        // Arrange
        _serviceDetail.IncludeScopesInAuthorizationRequest = true;
        _serviceDetail.Scopes = "./default";

        var sut = new AuthorizationParametersBuilder();

        // Act
        Dictionary<string, string> result = sut.BuildParametersForOAuth2ClientCredentials(_serviceDetail);

        // Assert
        result.Count.Should().Be(4);
        result["grant_type"].Should().Be("client_credentials");
        result["scope"].Should().Be("./default");
    }

    [Test]
    public void BuildParameters_ForOAuth2ClientCredentials_ReturnsExpectedResult()
    {
        // Arrange
        var sut = new AuthorizationParametersBuilder();

        // Act
        Dictionary<string, string> result = sut.BuildParametersForOAuth2ClientCredentials(_serviceDetail);

        // Assert
        result.Count.Should().Be(3);
        result["grant_type"].Should().Be("client_credentials");
        result["client_id"].Should().Be("TestClientId");
        result["client_secret"].Should().Be("TestClientSecret");
    }

    [Test]
    public void BuildParameters_ForOAuth2ClientCredentials_WithClientCredentialsProvisionInAuthHeader_ReturnsExpectedResult()
    {
        // Arrange
        _serviceDetail.ClientCredentialsProvision = ClientCredentialsProvision.AuthHeader;

        var sut = new AuthorizationParametersBuilder();

        // Act
        Dictionary<string, string> result = sut.BuildParametersForOAuth2ClientCredentials(_serviceDetail);

        // Assert
        result.Count.Should().Be(1);
        result["grant_type"].Should().Be("client_credentials");
    }


}
