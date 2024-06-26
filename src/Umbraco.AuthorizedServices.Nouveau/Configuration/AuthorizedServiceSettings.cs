using Umbraco.Cms.Core.Serialization;

namespace Umbraco.AuthorizedServices.Configuration;

/// <summary>
/// Defines options for token request formats.
/// </summary>
public enum TokenRequestContentFormat
{
    Querystring,
    FormUrlEncoded
}

/// <summary>
/// Defines options for the JSON serializer to use when building requests and deserializing responses.
/// </summary>
public enum JsonSerializerOption
{
    /// <summary>
    /// Uses the default Umbraco JSON serializer registered as <see cref="IJsonSerializer" />.
    /// </summary>
    Default,

    /// <summary>
    /// Uses JSON.Net for serialization.
    /// </summary>
    JsonNet,

    /// <summary>
    /// Uses System.Text.Json for serialization.
    /// </summary>
    SystemTextJson
}

/// <summary>
/// Defines the available authentication methods.
/// </summary>
public enum AuthenticationMethod
{
    OAuth1,
    OAuth2AuthorizationCode,
    OAuth2ClientCredentials,
    ApiKey
}

/// <summary>
/// Defines the available options for passing the API key with the request.
/// </summary>
public enum ApiKeyProvisionMethod
{
    HttpHeader,
    QueryString
}

/// <summary>
/// Defines the available provisioning methods for an OAuth2 Client Credentials flow.
/// </summary>
public enum ClientCredentialsProvision
{
    AuthHeader,
    RequestBody
}

/// <summary>
/// Defines the strongly typed configuration model.
/// </summary>
public class AuthorizedServiceSettings
{
    public string TokenEncryptionKey { get; set; } = string.Empty;

    public IDictionary<string, ServiceSummary> Services { get; set; } = new Dictionary<string, ServiceSummary>();
}

/// <summary>
/// Defines the provisioning options for an API key based authentication.
/// </summary>
public class ApiKeyProvision
{
    public ApiKeyProvisionMethod Method { get; set; }

    public string Key { get; set; } = string.Empty;

    public IDictionary<string, string> AdditionalParameters { get; set; } = new Dictionary<string, string>();

    public override string ToString() => $"{Method} / {Key}";
}

/// <summary>
/// Defines the provisioning options for an API exchanging short lived tokens with long lived ones.
/// </summary>
public class ExchangeTokenProvision
{
    public string TokenHost { get; set;} = string.Empty;

    public string RequestTokenPath { get; set;} = string.Empty;

    public string TokenGrantType { get; set; } = string.Empty;

    public string RequestRefreshTokenPath { get; set; } = string.Empty;

    public string RefreshTokenGrantType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the time interval for expiration of exchange tokens.
    /// </summary>
    public TimeSpan ExchangeTokenWhenExpiresWithin { get; set; } = TimeSpan.FromDays(30);
}

/// <summary>
/// Defines the strongly typed configuration for a single service.
/// </summary>
public class ServiceSummary
{
    /// <summary>
    /// Gets or sets the service alias.
    /// </summary>
    public string Alias { get; internal set; } = string.Empty;

    /// <summary>
    /// Gets or sets the display name for the service.
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the service icon.
    /// </summary>
    public string Icon { get; set; } = "icon-command";
}

/// <inheritdoc />
public class ServiceDetail : ServiceSummary
{
    /// <summary>
    /// Gets or sets the authentication method for the service.
    /// </summary>
    public AuthenticationMethod AuthenticationMethod { get; set; } = AuthenticationMethod.OAuth2AuthorizationCode;

    /// <summary>
    /// Gets or sets the provisioning type for an OAuth2 Client Credentials flow.
    /// </summary>
    public ClientCredentialsProvision ClientCredentialsProvision { get; set; } = ClientCredentialsProvision.RequestBody;

    /// <summary>
    /// Gets or sets the host name for the service's API.
    /// </summary>
    public string ApiHost { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the path for requests for authentication with the service.
    /// </summary>
    public string IdentityHost { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the path for requests for working with service tokens.
    /// </summary>
    public string TokenHost { get; set; } = string.Empty;

    /// <summary>
    /// Get or sets a value indicating whether an administrator can manually provide tokens via the backoffice.
    /// </summary>
    public bool CanManuallyProvideToken { get; set; }

    /// <summary>
    /// Get or sets a value indicating whether an administrator can manually provide API keys via the backoffice.
    /// </summary>
    public bool CanManuallyProvideApiKey { get; set; }

    /// <summary>
    /// Gets or sets the path for requests for obtaining authorization for a user in OAuth1 flows.
    /// </summary>
    public string RequestAuthorizationPath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the path for requests for authentication with the service.
    /// </summary>
    public string RequestIdentityPath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether authorization requests require sending the redirect URL.
    /// </summary>
    public bool AuthorizationUrlRequiresRedirectUrl { get; set; } = false;

    /// <summary>
    /// Gets or sets the path for requests for requesting a token from the service.
    /// </summary>
    public string RequestTokenPath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the HTTP method used to retrieve the access token.
    /// </summary>
    public HttpMethod RequestTokenMethod { get; set; } = HttpMethod.Post;

    /// <summary>
    /// Gets or sets the format to use for encoding the request for a token.
    /// </summary>
    public TokenRequestContentFormat? RequestTokenFormat { get; set; }

    /// <summary>
    /// Gets or sets the JSON serializer to use when building requests and deserializing responses.
    /// </summary>
    public JsonSerializerOption JsonSerializer { get; set; } = JsonSerializerOption.Default;

    /// <summary>
    /// Gets or sets a value indicating whether the basic token should be included in the token request.
    /// </summary>
    public bool AuthorizationRequestRequiresAuthorizationHeaderWithBasicToken { get; set; } = false;

    /// <summary>
    /// Gets or sets the API Key for the service.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the provisioning options for an API key based authentication.
    /// </summary>
    public ApiKeyProvision? ApiKeyProvision { get; set; }

    /// <summary>
    /// Gets or sets the client Id for the app registered with the service.
    /// </summary>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the client secret for the app registered with the service.
    /// </summary>
    public string ClientSecret { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the scopes required for working with the service.
    /// </summary>
    public string Scopes { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether the scopes should be included in the authorization request body.
    /// </summary>
    public bool IncludeScopesInAuthorizationRequest { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the OAuth flow should use Proof of Key Code Exchange (PKCE).
    /// </summary>
    public bool UseProofKeyForCodeExchange { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the OAuth flow should exchange the access token.
    /// </summary>
    public bool CanExchangeToken { get; set; }

    /// <summary>
    /// Gets or sets the provisioning options for exchanging token flow.
    /// </summary>
    public ExchangeTokenProvision? ExchangeTokenProvision { get; set; }

    /// <summary>
    /// Gets or sets the key expected in the token response that identifies the access token.
    /// </summary>
    public string AccessTokenResponseKey { get; set; } = "access_token";

    /// <summary>
    /// Gets or sets the key expected in the token response that identifies the refresh token.
    /// </summary>
    public string RefreshTokenResponseKey { get; set; } = "refresh_token";

    /// <summary>
    /// Gets or sets the key expected in the token response that identifies when the accesstoken expires.
    /// </summary>
    public string ExpiresInResponseKey { get; set; } = "expires_in";

    /// <summary>
    /// Gets or sets the path to a GET request used as a sample for verifying the service in the backoffice.
    /// </summary>
    public string? SampleRequest { get; set; }

    /// <summary>
    /// Gets or sets the time interval for expiration of access tokens.
    /// </summary>
    public TimeSpan RefreshAccessTokenWhenExpiresWithin { get; set; } = TimeSpan.FromSeconds(30);

    internal string GetTokenHost() => string.IsNullOrWhiteSpace(TokenHost)
        ? IdentityHost
        : TokenHost;
}
