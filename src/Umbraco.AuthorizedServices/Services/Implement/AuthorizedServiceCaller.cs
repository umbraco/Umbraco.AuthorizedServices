using Azure.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.AuthorizedServices.Configuration;
using Umbraco.AuthorizedServices.Exceptions;
using Umbraco.AuthorizedServices.Helpers;
using Umbraco.AuthorizedServices.Models;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Extensions;

namespace Umbraco.AuthorizedServices.Services.Implement;

internal sealed class AuthorizedServiceCaller : AuthorizedServiceBase, IAuthorizedServiceCaller
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly JsonSerializerFactory _jsonSerializerFactory;
    private readonly IAuthorizationUrlBuilder _authorizationUrlBuilder;
    private readonly IAuthorizedRequestBuilder _authorizedRequestBuilder;
    private readonly IRefreshTokenParametersBuilder _refreshTokenParametersBuilder;
    private readonly IExchangeTokenParametersBuilder _exchangeTokenParametersBuilder;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuthorizedServiceCaller(
        AppCaches appCaches,
        ITokenFactory tokenFactory,
        IOAuth2TokenStorage oauth2TokenStorage,
        IOAuth1TokenStorage oauth1TokenStorage,
        IKeyStorage keyStorage,
        IAuthorizationRequestSender authorizationRequestSender,
        ILogger<AuthorizedServiceCaller> logger,
        IOptionsMonitor<ServiceDetail> serviceDetailOptions,
        IHttpClientFactory httpClientFactory,
        JsonSerializerFactory jsonSerializerFactory,
        IAuthorizationUrlBuilder authorizationUrlBuilder,
        IAuthorizedRequestBuilder authorizedRequestBuilder,
        IRefreshTokenParametersBuilder refreshTokenParametersBuilder,
        IExchangeTokenParametersBuilder exchangeTokenParametersBuilder,
        IHttpContextAccessor httpContextAccessor)
        : base(appCaches, tokenFactory, oauth2TokenStorage, oauth1TokenStorage, keyStorage, authorizationRequestSender, logger, serviceDetailOptions)
    {
        _httpClientFactory = httpClientFactory;
        _jsonSerializerFactory = jsonSerializerFactory;
        _authorizationUrlBuilder = authorizationUrlBuilder;
        _authorizedRequestBuilder = authorizedRequestBuilder;
        _refreshTokenParametersBuilder = refreshTokenParametersBuilder;
        _exchangeTokenParametersBuilder = exchangeTokenParametersBuilder;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<Attempt<EmptyResponse?>> SendRequestAsync(string serviceAlias, string path, HttpMethod httpMethod)
      => await SendRequestAsync<object, EmptyResponse>(serviceAlias, path, httpMethod, null);

    public async Task<Attempt<TResponse?>> SendRequestAsync<TResponse>(string serviceAlias, string path, HttpMethod httpMethod)
      => await SendRequestAsync<object, TResponse>(serviceAlias, path, httpMethod, null);

    public async Task<Attempt<TResponse?>> SendRequestAsync<TRequest, TResponse>(string serviceAlias, string path, HttpMethod httpMethod, TRequest? requestContent = null)
        where TRequest : class
    {
        Attempt<string?> responseContentAttempt = await SendRequestRawAsync(serviceAlias, path, httpMethod, requestContent);

        if (!responseContentAttempt.Success)
        {
            if (responseContentAttempt.Exception is null)
            {
                return Attempt.Fail<TResponse?>();
            }

            return Attempt.Fail<TResponse?>(
                default,
                responseContentAttempt.Exception);
        }

        var responseContent = responseContentAttempt.Result;
        if (!string.IsNullOrWhiteSpace(responseContent))
        {
            IJsonSerializer jsonSerializer = _jsonSerializerFactory.GetSerializer(serviceAlias);
            TResponse? result = jsonSerializer.Deserialize<TResponse>(responseContent);
            if (result != null)
            {
                return Attempt.Succeed(result);
            }

            return Attempt.Fail<TResponse?>(
                default,
                new AuthorizedServiceException($"Could not deserialize result of request to service '{serviceAlias}'"));
        }

        return Attempt.Succeed<TResponse?>(default);
    }

    public async Task<Attempt<string?>> SendRequestRawAsync(string serviceAlias, string path, HttpMethod httpMethod)
      => await SendRequestRawAsync<object>(serviceAlias, path, httpMethod, null);

    public async Task<Attempt<string?>> SendRequestRawAsync<TRequest>(string serviceAlias, string path, HttpMethod httpMethod, TRequest? requestContent = null)
        where TRequest : class
    {
        ServiceDetail serviceDetail = GetServiceDetail(serviceAlias);

        HttpClient httpClient = _httpClientFactory.CreateClient();

        Attempt<HttpRequestMessage?> requestMessageAttempt = await CreateHttpRequestMessage(serviceAlias, path, httpMethod, requestContent, serviceDetail);
        if (!requestMessageAttempt.Success)
        {
            return Attempt.Fail<string?>(
                default,
                requestMessageAttempt.Exception!);
        }

        HttpResponseMessage response = await httpClient.SendAsync(requestMessageAttempt.Result!);

        if (response.IsSuccessStatusCode)
        {
            return Attempt.Succeed(await response.Content.ReadAsStringAsync());
        }

        return Attempt.Fail<string?>(
            default,
            new AuthorizedServiceHttpException(
                $"Error response received from request to '{serviceAlias}'.",
                response.StatusCode,
                response.ReasonPhrase,
                await response.Content.ReadAsStringAsync()));
    }

    private async Task<Attempt<HttpRequestMessage?>> CreateHttpRequestMessage<TRequest>(string serviceAlias, string path, HttpMethod httpMethod, TRequest? requestContent, ServiceDetail serviceDetail) where TRequest : class
    {
        switch (serviceDetail.AuthenticationMethod)
        {
            case AuthenticationMethod.ApiKey:
                return CreateHttpRequestMessageForApiKeyAuthentication(serviceAlias, path, httpMethod, requestContent, serviceDetail);
            case AuthenticationMethod.OAuth1:
                return CreateHttpRequestMessageForOAuth1Authentication(serviceAlias, path, httpMethod, requestContent, serviceDetail);
            case AuthenticationMethod.OAuth2AuthorizationCode:
            case AuthenticationMethod.OAuth2ClientCredentials:
                return await CreateRequestMessageForOAuth2Authentication(serviceAlias, path, httpMethod, requestContent, serviceDetail);
            default:
                throw new ArgumentOutOfRangeException(nameof(serviceDetail.AuthenticationMethod));
        }
    }

    private Attempt<HttpRequestMessage?> CreateHttpRequestMessageForApiKeyAuthentication<TRequest>(string serviceAlias, string path, HttpMethod httpMethod, TRequest? requestContent, ServiceDetail serviceDetail) where TRequest : class
    {
        var apiKey = !string.IsNullOrEmpty(serviceDetail.ApiKey)
            ? serviceDetail.ApiKey
            : GetApiKeyFromCacheOrStorage(serviceAlias);

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return Attempt.Fail<HttpRequestMessage?>(
                default,
                new AuthorizedServiceException($"Cannot request service '{serviceAlias}' as access has not yet been authorized (no API key is configured or stored)."));
        }

        HttpRequestMessage requestMessage = _authorizedRequestBuilder.CreateRequestMessageWithApiKey(
            serviceDetail,
            path,
            httpMethod,
            apiKey,
            requestContent);
        return Attempt.Succeed(requestMessage);
    }


    private Attempt<HttpRequestMessage?> CreateHttpRequestMessageForOAuth1Authentication<TRequest>(string serviceAlias, string path, HttpMethod httpMethod, TRequest? requestContent, ServiceDetail serviceDetail) where TRequest : class
    {
        OAuth1Token? token = GetOAuth1TokenFromCacheOrStorage(serviceAlias);
        if (token is null)
        {
            return Attempt.Fail<HttpRequestMessage?>(
                default,
                new AuthorizedServiceException($"Cannot request service '{serviceAlias}' as access has not yet been authorized (no OAuth token is available)."));
        }

        HttpRequestMessage requestMessage = _authorizedRequestBuilder.CreateRequestMessageWithOAuth1Token(serviceDetail, path, httpMethod, token, requestContent);
        return Attempt.Succeed(requestMessage);
    }

    private async Task<Attempt<HttpRequestMessage?>> CreateRequestMessageForOAuth2Authentication<TRequest>(string serviceAlias, string path, HttpMethod httpMethod, TRequest? requestContent, ServiceDetail serviceDetail) where TRequest : class
    {
        OAuth2Token? token = GetOAuth2TokenFromCacheOrStorage(serviceAlias);
        if (token is null)
        {
            return Attempt.Fail<HttpRequestMessage?>(
                default,
                new AuthorizedServiceException($"Cannot request service '{serviceAlias}' as access has not yet been authorized (no access token is available)."));
        }

        token = serviceDetail.CanExchangeToken
            ? await EnsureExchangeAccessToken(serviceAlias, token)
            : await EnsureAccessToken(serviceAlias, token);

        HttpRequestMessage requestMessage = _authorizedRequestBuilder.CreateRequestMessageWithOAuth2Token(serviceDetail, path, httpMethod, token, requestContent);
        return Attempt.Succeed(requestMessage);
    }

    public Attempt<string?> GetApiKey(string serviceAlias)
    {
        // First check for configured API key.
        var apiKey = GetServiceDetail(serviceAlias)?.ApiKey;
        if (!string.IsNullOrWhiteSpace(apiKey))
        {
            return Attempt.Succeed(apiKey);
        }

        // If not found, look for stored key.
        apiKey = KeyStorage.GetKey(serviceAlias);
        if (!string.IsNullOrWhiteSpace(apiKey))
        {
            return Attempt.Succeed(apiKey);
        }

        return Attempt.Fail<string?>();
    }

    public Attempt<string?> GetOAuth2AccessToken(string serviceAlias)
    {
        var accessToken = GetOAuth2TokenFromCacheOrStorage(serviceAlias)?.AccessToken;
        if (!string.IsNullOrWhiteSpace(accessToken))
        {
            return Attempt.Succeed(accessToken);
        }

        return Attempt.Fail<string?>();
    }

    public Attempt<string?> GetOAuth1Token(string serviceAlias)
    {
        var token = GetOAuth1TokenFromCacheOrStorage(serviceAlias)?.OAuthToken;
        if (!string.IsNullOrWhiteSpace(token))
        {
            return Attempt.Succeed(token);
        }

        return Attempt.Fail<string?>();
    }

    private async Task<OAuth2Token> EnsureAccessToken(string serviceAlias, OAuth2Token token)
    {
        ServiceDetail serviceDetail = GetServiceDetail(serviceAlias);
        if (token.WillBeExpiredAfter(serviceDetail.RefreshAccessTokenWhenExpiresWithin))
        {
            if (string.IsNullOrEmpty(token.RefreshToken))
            {
                ClearOAuth2AccessToken(serviceAlias);
                throw new AuthorizedServiceException($"Cannot request service '{serviceAlias}' as the access token has or will expire and no refresh token is available to use. The expired token has been deleted.");
            }

            return await RefreshAccessToken(serviceAlias, token.RefreshToken)
                ?? throw new AuthorizedServiceException($"Cannot request service '{serviceAlias}' as the access token has or will expired and the refresh token could not be used to obtain a new access token.");
        }

        return token;
    }

    private async Task<OAuth2Token?> RefreshAccessToken(string serviceAlias, string refreshToken)
    {
        ServiceDetail serviceDetail = GetServiceDetail(serviceAlias);

        Dictionary<string, string> parameters = _refreshTokenParametersBuilder.BuildParameters(serviceDetail, refreshToken);

        HttpResponseMessage response = await AuthorizationRequestSender.SendOAuth2Request(serviceDetail, parameters);
        if (response.IsSuccessStatusCode)
        {
            OAuth2Token token = await CreateOAuth2TokenFromResponse(serviceDetail, response);
            StoreOAuth2Token(serviceAlias, token);
            return token;
        }
        else
        {
            throw new AuthorizedServiceHttpException(
                $"Error response from refresh token request to '{serviceAlias}'.",
                response.StatusCode,
                response.ReasonPhrase,
                await response.Content.ReadAsStringAsync());
        }
    }

    private async Task<OAuth2Token> EnsureExchangeAccessToken(string serviceAlias, OAuth2Token token)
    {
        ServiceDetail serviceDetail = GetServiceDetail(serviceAlias);
        if (serviceDetail.ExchangeTokenProvision is not null
            && token.WillBeExpiredAfter(serviceDetail.ExchangeTokenProvision.ExchangeTokenWhenExpiresWithin))
        {
            return await RefreshExchangeAccessToken(serviceAlias, token.AccessToken)
                ?? throw new AuthorizedServiceException($"Cannot request service '{serviceAlias}' as the access token will expire and it's not been possible to exchange it for a new access token.");
        }

        return token;
    }

    private async Task<OAuth2Token?> RefreshExchangeAccessToken(string serviceAlias, string accessToken)
    {
        ServiceDetail serviceDetail = GetServiceDetail(serviceAlias);

        Dictionary<string, string> parameters = _exchangeTokenParametersBuilder.BuildParameters(serviceDetail, accessToken);

        HttpResponseMessage response = await AuthorizationRequestSender.SendOAuth2ExchangeRequest(serviceDetail, parameters);
        if (response.IsSuccessStatusCode)
        {
            OAuth2Token token = await CreateOAuth2TokenFromResponse(serviceDetail, response);
            StoreOAuth2Token(serviceAlias, token);
            return token;
        }
        else
        {
            throw new AuthorizedServiceHttpException(
                $"Error response from exchange access token request to '{serviceAlias}'.",
                response.StatusCode,
                response.ReasonPhrase,
                await response.Content.ReadAsStringAsync());
        }
    }

    private string? GetApiKeyFromCacheOrStorage(string serviceAlias)
    {
        // First look in cache.
        var cacheKey = CacheHelper.GetApiKeyCacheKey(serviceAlias);
        string? apiKey = AppCaches.RuntimeCache.GetCacheItem<string>(cacheKey);
        if (!string.IsNullOrWhiteSpace(apiKey))
        {
            return apiKey;
        }

        // Second, look in storage, and if found, save to cache.
        apiKey = KeyStorage.GetKey(serviceAlias);
        if (!string.IsNullOrWhiteSpace(apiKey))
        {
            AppCaches.RuntimeCache.InsertCacheItem(cacheKey, () => apiKey);
            return apiKey;
        }

        return null;
    }

    private OAuth2Token? GetOAuth2TokenFromCacheOrStorage(string serviceAlias)
    {
        // First look in cache.
        var cacheKey = CacheHelper.GetTokenCacheKey(serviceAlias);
        OAuth2Token? token = AppCaches.RuntimeCache.GetCacheItem<OAuth2Token>(cacheKey);
        if (token != null)
        {
            return token;
        }

        // Second, look in storage, and if found, save to cache.
        token = OAuth2TokenStorage.GetToken(serviceAlias);
        if (token != null)
        {
            AppCaches.RuntimeCache.InsertCacheItem(cacheKey, () => token);
            return token;
        }

        return null;
    }

    private OAuth1Token? GetOAuth1TokenFromCacheOrStorage(string serviceAlias)
    {
        // First look in cache.
        var cacheKey = CacheHelper.GetTokenCacheKey(serviceAlias);
        OAuth1Token? token = AppCaches.RuntimeCache.GetCacheItem<OAuth1Token>(cacheKey);
        if (token != null)
        {
            return token;
        }

        // Second, look in storage, and if found, save to cache.
        token = OAuth1TokenStorage.GetToken(serviceAlias);
        if (token != null)
        {
            AppCaches.RuntimeCache.InsertCacheItem(cacheKey, () => token);
            return token;
        }

        return null;
    }

    private void ClearOAuth2AccessToken(string serviceAlias)
    {
        var cacheKey = CacheHelper.GetTokenCacheKey(serviceAlias);
        AppCaches.RuntimeCache.ClearByKey(cacheKey);
        OAuth2TokenStorage.DeleteToken(serviceAlias);
    }
}
