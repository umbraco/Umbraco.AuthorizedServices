using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.AuthorizedServices.Configuration;
using Umbraco.AuthorizedServices.Models;
using Umbraco.Cms.Core.Cache;
using Umbraco.Extensions;

namespace Umbraco.AuthorizedServices.Services.Implement;

internal abstract class AuthorizedServiceBase
{
    private readonly IOptionsMonitor<ServiceDetail> _serviceDetailOptions;
    private readonly ITokenFactory _tokenFactory;

    public AuthorizedServiceBase(
        AppCaches appCaches,
        ITokenFactory tokenFactory,
        IOAuth2TokenStorage oauth2TokenStorage,
        IOAuth1TokenStorage oauth1TokenStorage,
        IKeyStorage keyStorage,
        IAuthorizationRequestSender authorizationRequestSender,
        ILogger logger,
        IOptionsMonitor<ServiceDetail> serviceDetailOptions)
    {
        AppCaches = appCaches;
        _tokenFactory = tokenFactory;
        OAuth2TokenStorage = oauth2TokenStorage;
        OAuth1TokenStorage = oauth1TokenStorage;
        KeyStorage = keyStorage;
        AuthorizationRequestSender = authorizationRequestSender;
        Logger = logger;
        _serviceDetailOptions = serviceDetailOptions;
    }

    protected AppCaches AppCaches { get; }

    protected IOAuth2TokenStorage OAuth2TokenStorage { get; }

    protected IOAuth1TokenStorage OAuth1TokenStorage { get; }

    protected IKeyStorage KeyStorage { get; }

    protected IAuthorizationRequestSender AuthorizationRequestSender { get; }

    protected ILogger Logger { get; }

    protected ServiceDetail GetServiceDetail(string serviceAlias) => _serviceDetailOptions.Get(serviceAlias);

    protected async Task<OAuth2Token> CreateOAuth2TokenFromResponse(ServiceDetail serviceDetail, HttpResponseMessage response)
    {
        var responseContent = await response.Content.ReadAsStringAsync();
        return _tokenFactory.CreateFromOAuth2ResponseContent(responseContent, serviceDetail);
    }

    protected async Task<OAuth1Token> CreateOAuth1TokenFromResponse(HttpResponseMessage response)
    {
        var responseContent = await response.Content.ReadAsStringAsync();
        return _tokenFactory.CreateFromOAuth1ResponseContent(responseContent);
    }

    protected OAuth2Token? GetStoredToken(string serviceAlias) => OAuth2TokenStorage.GetToken(serviceAlias);

    protected void StoreOAuth2Token(string serviceAlias, OAuth2Token token)
    {
        // Add the access token details to the cache.
        var cacheKey = GetTokenCacheKey(serviceAlias);
        AppCaches.RuntimeCache.InsertCacheItem(cacheKey, () => token);

        // Save the refresh token into the storage.
        OAuth2TokenStorage.SaveToken(serviceAlias, token);
    }

    protected void StoreOAuth1Token(string serviceAlias, OAuth1Token token)
    {
        // Save the token information into the storage.
        OAuth1TokenStorage.SaveToken(serviceAlias, token);
    }

    private static string GetTokenCacheKey(string serviceAlias) => $"Umbraco_AuthorizedServiceToken_{serviceAlias}";
}
