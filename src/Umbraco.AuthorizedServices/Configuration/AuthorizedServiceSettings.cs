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
/// Defines the strongly typed configuration model.
/// </summary>
public class AuthorizedServiceSettings
{
    public List<ServiceDetail> Services { get; set; } = new List<ServiceDetail>();
}

/// <summary>
/// Defines the strongly typed configuration for a single service.
/// </summary>
public class ServiceDetail
{
    /// <summary>
    /// Gets or sets the service alias.
    /// </summary>
    public string Alias { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the display name for the service.
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the service icon.
    /// </summary>
    public string Icon { get; set; } = "icon-command";

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
    /// Gets or sets the path for requests for authentication with the service.
    /// </summary>
    public string RequestIdentityPath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a vallue indicating whether authorization requests require sending the redirect URL.
    /// </summary>
    public bool AuthorizationRequestsRequireRedirectUri { get; set; } = false;

    /// <summary>
    /// Gets or sets the path for requests for requesting a token from the service.
    /// </summary>
    public string RequestTokenPath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the format to use for encoding the request for a token.
    /// </summary>
    public TokenRequestContentFormat RequestTokenFormat { get; set; } = TokenRequestContentFormat.Querystring;

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
    /// Gets or sets a value indicating whether the OAuth flow should use with PKCE.
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
