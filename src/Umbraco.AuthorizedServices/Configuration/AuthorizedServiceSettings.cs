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
/// Defines the strongly typed configuration model.
/// </summary>
public class AuthorizedServiceSettings
{
    public string TokenEncryptionKey { get; set; } = string.Empty;

    public IDictionary<string, ServiceSummary> Services { get; set; } = new Dictionary<string, ServiceSummary>();
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
    /// Get or sets a value indicating whether editor can manually add token.
    /// </summary>
    public bool CanManuallyProvideToken { get; set; }

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
    /// Gets or sets the format to use for encoding the request for a token.
    /// </summary>
    public TokenRequestContentFormat RequestTokenFormat { get; set; } = TokenRequestContentFormat.Querystring;

    /// <summary>
    /// Gets or sets the JSON serializer to use when building requests and deserializing responses.
    /// </summary>
    public JsonSerializerOption JsonSerializer { get; set; } = JsonSerializerOption.Default;

    /// <summary>
    /// Gets or sets a value indicating whether the basic token should be included in the token request.
    /// </summary>
    public bool AuthorizationRequestRequiresAuthorizationHeaderWithBasicToken { get; set; } = false;

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
    /// Gets or sets a value indicating whether the OAuth flow should use Proof of Key Code Exchange (PKCE).
    /// </summary>
    public bool UseProofKeyForCodeExchange { get; set; }

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

    internal string GetTokenHost() => string.IsNullOrWhiteSpace(TokenHost)
        ? IdentityHost
        : TokenHost;
}
