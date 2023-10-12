using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.AuthorizedServices.Configuration;
using Umbraco.AuthorizedServices.Exceptions;
using Umbraco.AuthorizedServices.Helpers;
using Umbraco.AuthorizedServices.Models;
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

    public async Task SendRequestAsync(string serviceAlias, string path, HttpMethod httpMethod)
      => await SendRequestAsync<EmptyResponse, object>(serviceAlias, path, httpMethod, null);

    public async Task<TResponse?> SendRequestAsync<TResponse>(string serviceAlias, string path, HttpMethod httpMethod)
      => await SendRequestAsync<EmptyResponse, TResponse>(serviceAlias, path, httpMethod, null);

    public async Task<TResponse?> SendRequestAsync<TRequest, TResponse>(string serviceAlias, string path, HttpMethod httpMethod, TRequest? requestContent = null)
        where TRequest : class
    {
        string responseContent = await SendRequestRawAsync(serviceAlias, path, httpMethod, requestContent);

        if (typeof(TResponse) == typeof(EmptyResponse))
        {
            return default;
        }

        if (!string.IsNullOrWhiteSpace(responseContent))
        {
            IJsonSerializer jsonSerializer = _jsonSerializerFactory.GetSerializer(serviceAlias);
            TResponse? result = jsonSerializer.Deserialize<TResponse>(responseContent);
            if (result != null)
            {
                return result;
            }

            throw new AuthorizedServiceException($"Could not deserialize result of request to service '{serviceAlias}'");
        }

        return default;
    }

    public async Task<string> SendRequestRawAsync(string serviceAlias, string path, HttpMethod httpMethod)
      => await SendRequestRawAsync<object>(serviceAlias, path, httpMethod, null);

    public async Task<string> SendRequestRawAsync<TRequest>(string serviceAlias, string path, HttpMethod httpMethod, TRequest? requestContent = null)
        where TRequest : class
    {
        ServiceDetail serviceDetail = GetServiceDetail(serviceAlias);

        HttpClient httpClient = _httpClientFactory.CreateClient();

        HttpRequestMessage requestMessage;
        if (serviceDetail.AuthenticationMethod == AuthenticationMethod.ApiKey)
        {
            var apiKey = !string.IsNullOrEmpty(serviceDetail.ApiKey)
                ? serviceDetail.ApiKey
                : KeyStorage.GetKey(serviceAlias);

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new AuthorizedServiceException($"Cannot request service '{serviceAlias}' as access has not yet been authorized (no API key is configured or stored).");
            }

            requestMessage = _authorizedRequestBuilder.CreateRequestMessageWithApiKey(
                serviceDetail,
                path,
                httpMethod,
                apiKey,
                requestContent);
        }
        else if (serviceDetail.AuthenticationMethod == AuthenticationMethod.OAuth1)
        {
            OAuth1Token? token = GetOAuth1TokenFromCacheOrStorage(serviceAlias) ?? throw new AuthorizedServiceException($"Cannot request service '{serviceAlias}' as access has not yet been authorized (no OAuth token is available).");

            requestMessage = _authorizedRequestBuilder.CreateRequestMessageWithOAuth1Token(serviceDetail, path, httpMethod, token, requestContent);
        }
        else
        {
            OAuth2Token? token = GetOAuth2AccessTokenFromCacheOrStorage(serviceAlias) ?? throw new AuthorizedServiceException($"Cannot request service '{serviceAlias}' as access has not yet been authorized (no access token is available).");

            token = serviceDetail.CanExchangeToken
                ? await EnsureExchangeAccessToken(serviceAlias, token)
                : await EnsureAccessToken(serviceAlias, token);

            requestMessage = _authorizedRequestBuilder.CreateRequestMessageWithOAuth2Token(serviceDetail, path, httpMethod, token, requestContent);
        }

        HttpResponseMessage response = await httpClient.SendAsync(requestMessage);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadAsStringAsync();
        }

        throw new AuthorizedServiceHttpException(
            $"Error response received from request to '{serviceAlias}'.",
            response.StatusCode,
            response.ReasonPhrase,
            await response.Content.ReadAsStringAsync());
    }

    public string? GetApiKey(string serviceAlias)
    {
        ServiceDetail serviceDetail = GetServiceDetail(serviceAlias);
        return !string.IsNullOrEmpty(serviceDetail.ApiKey)
            ? serviceDetail.ApiKey
            : KeyStorage.GetKey(serviceAlias);
    }

    public string? GetOAuth2AccessToken(string serviceAlias)
    {
        OAuth2Token? token = GetOAuth2AccessTokenFromCacheOrStorage(serviceAlias);
        return token?.AccessToken;
    }

    public string? GetOAuth1Token(string serviceAlias)
    {
        OAuth1Token? token = GetOAuth1TokenFromCacheOrStorage(serviceAlias);
        return token?.OAuthToken;
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

    private OAuth2Token? GetOAuth2AccessTokenFromCacheOrStorage(string serviceAlias)
    {
        // First look in cache.
        var cacheKey = GetTokenCacheKey(serviceAlias);
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
        var cacheKey = GetTokenCacheKey(serviceAlias);
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
        AppCaches.RuntimeCache.ClearByKey(GetTokenCacheKey(serviceAlias));
        OAuth2TokenStorage.DeleteToken(serviceAlias);
    }

    private static string GetTokenCacheKey(string serviceAlias) => string.Format(Constants.Cache.AuthorizationTokenFormat, serviceAlias);

    private class EmptyResponse
    {
    }
}
