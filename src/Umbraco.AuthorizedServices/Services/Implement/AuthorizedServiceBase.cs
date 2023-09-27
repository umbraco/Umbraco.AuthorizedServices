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
        ITokenStorage tokenStorage,
        IKeyStorage keyStorage,
        IAuthorizationRequestSender authorizationRequestSender,
        ILogger logger,
        IOptionsMonitor<ServiceDetail> serviceDetailOptions)
    {
        AppCaches = appCaches;
        _tokenFactory = tokenFactory;
        TokenStorage = tokenStorage;
        KeyStorage = keyStorage;
        AuthorizationRequestSender = authorizationRequestSender;
        Logger = logger;
        _serviceDetailOptions = serviceDetailOptions;
    }

    protected AppCaches AppCaches { get; }

    protected ITokenStorage TokenStorage { get; }

    protected IKeyStorage KeyStorage { get; }

    protected IAuthorizationRequestSender AuthorizationRequestSender { get; }

    protected ILogger Logger { get; }

    protected ServiceDetail GetServiceDetail(string serviceAlias) => _serviceDetailOptions.Get(serviceAlias);

    protected async Task<Token> CreateTokenFromResponse(ServiceDetail serviceDetail, HttpResponseMessage response)
    {
        var responseContent = await response.Content.ReadAsStringAsync();
        return _tokenFactory.CreateFromResponseContent(responseContent, serviceDetail);
    }

    protected Token? GetStoredToken(string serviceAlias) => TokenStorage.GetToken(serviceAlias);

    protected void StoreToken(string serviceAlias, Token token)
    {
        // Add the access token details to the cache.
        var cacheKey = GetTokenCacheKey(serviceAlias);
        AppCaches.RuntimeCache.InsertCacheItem(cacheKey, () => token);

        // Save the refresh token into the storage.
        TokenStorage.SaveToken(serviceAlias, token);
    }

    private static string GetTokenCacheKey(string serviceAlias) => $"Umbraco_AuthorizedServiceToken_{serviceAlias}";
}
