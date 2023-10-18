using Umbraco.AuthorizedServices.Configuration;
using Umbraco.AuthorizedServices.Services.Implement;

namespace Umbraco.AuthorizedServices.Tests.Services;

internal class AuthorizationParametersBuilderTests
{
    private const string TestAuthorizationCode = "1234";
    private const string TestRedirectUrl = "https://test.url";
    private const string TestCodeVerifier = "TestCodeVerifier";

    [Test]
    public void BuildParametersForOAuth2AuthorizationCode_ReturnsExpectedResult()
    {
        // Arrange
        ServiceDetail serviceDetail = CreateServiceDetail();

        var sut = new AuthorizationParametersBuilder();

        // Act
        Dictionary<string, string> result = sut.BuildParametersForOAuth2AuthorizationCode(serviceDetail, TestAuthorizationCode, TestRedirectUrl, string.Empty);

        // Assert
        result.Count.Should().Be(5);
        AssertOAuth2AuthorizationCodeParameters(result);
    }

    [Test]
    public void BuildParametersForOAuth2AuthorizationCode_WithCodeVerifier_ReturnsExpectedResult()
    {
        // Arrange
        ServiceDetail serviceDetail = CreateServiceDetail();
        serviceDetail.UseProofKeyForCodeExchange = true;

        var sut = new AuthorizationParametersBuilder();

        // Act
        Dictionary<string, string> result = sut.BuildParametersForOAuth2AuthorizationCode(serviceDetail, TestAuthorizationCode, TestRedirectUrl, TestCodeVerifier);

        // Assert
        result.Count.Should().Be(6);
        AssertOAuth2AuthorizationCodeParameters(result);
        result["code_verifier"].Should().Be(TestCodeVerifier);
    }

    [Test]
    public void BuildParametersForOAuth2ClientCredentials_ReturnsExpectedResult()
    {
        // Arrange
        ServiceDetail serviceDetail = CreateServiceDetail();
        var sut = new AuthorizationParametersBuilder();

        // Act
        Dictionary<string, string> result = sut.BuildParametersForOAuth2ClientCredentials(serviceDetail);

        // Assert
        result.Count.Should().Be(3);
        AssertOAuth2ClientCredentialsParameters(result);
    }

    [Test]
    public void BuildParametersForOAuth2ClientCredentials_WithScopes_ReturnsExpectedResult()
    {
        // Arrange
        ServiceDetail serviceDetail = CreateServiceDetail();
        serviceDetail.IncludeScopesInAuthorizationRequest = true;
        serviceDetail.Scopes = "./default";

        var sut = new AuthorizationParametersBuilder();

        // Act
        Dictionary<string, string> result = sut.BuildParametersForOAuth2ClientCredentials(serviceDetail);

        // Assert
        result.Count.Should().Be(4);
        AssertOAuth2ClientCredentialsParameters(result);
        result["scope"].Should().Be("./default");
    }

    [Test]
    public void BuildParametersForOAuth2ClientCredentials_WithClientCredentialsProvisionInAuthHeader_ReturnsExpectedResult()
    {
        // Arrange
        ServiceDetail serviceDetail = CreateServiceDetail();
        serviceDetail.ClientCredentialsProvision = ClientCredentialsProvision.AuthHeader;

        var sut = new AuthorizationParametersBuilder();

        // Act
        Dictionary<string, string> result = sut.BuildParametersForOAuth2ClientCredentials(serviceDetail);

        // Assert
        result.Count.Should().Be(1);
        result["grant_type"].Should().Be("client_credentials");
    }

    [Test]
    public void BuildParametersForOAuth1_WithClientCredentialsProvisionInAuthHeader_ReturnsExpectedResult()
    {
        // Arrange
        ServiceDetail serviceDetail = CreateServiceDetail();
        serviceDetail.AuthenticationMethod = AuthenticationMethod.OAuth1;
        const string Token = "1234";
        const string Verifier = "5678";
        const string Secret = "90";

        var sut = new AuthorizationParametersBuilder();

        // Act
        Dictionary<string, string> result = sut.BuildParametersForOAuth1(serviceDetail, Token, Verifier, Secret);

        // Assert
        result.Count.Should().Be(8);
        result["oauth_token"].Should().Be(Token);
        result["oauth_verifier"].Should().Be(Verifier);
        result["oauth_consumer_key"].Should().Be("TestClientId");
    }

    private static ServiceDetail CreateServiceDetail() => new()
    {
        ClientId = "TestClientId",
        ClientSecret = "TestClientSecret",
    };

    private static void AssertOAuth2AuthorizationCodeParameters(Dictionary<string, string> result)
    {
        result["grant_type"].Should().Be("authorization_code");
        result["client_id"].Should().Be("TestClientId");
        result["client_secret"].Should().Be("TestClientSecret");
        result["code"].Should().Be(TestAuthorizationCode);
        result["redirect_uri"].Should().Be(TestRedirectUrl);
    }

    private static void AssertOAuth2ClientCredentialsParameters(Dictionary<string, string> result)
    {
        result["grant_type"].Should().Be("client_credentials");
        result["client_id"].Should().Be("TestClientId");
        result["client_secret"].Should().Be("TestClientSecret");
    }
}
