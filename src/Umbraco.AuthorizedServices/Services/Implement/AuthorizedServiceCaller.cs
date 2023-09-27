using System.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.AuthorizedServices.Configuration;
using Umbraco.AuthorizedServices.Exceptions;
using Umbraco.AuthorizedServices.Models;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Extensions;

namespace Umbraco.AuthorizedServices.Services.Implement;

internal sealed class AuthorizedServiceCaller : AuthorizedServiceBase, IAuthorizedServiceCaller
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly JsonSerializerFactory _jsonSerializerFactory;
    private readonly IAuthorizedRequestBuilder _authorizedRequestBuilder;
    private readonly IRefreshTokenParametersBuilder _refreshTokenParametersBuilder;
    private readonly IExchangeTokenParametersBuilder _exchangeTokenParametersBuilder;

    public AuthorizedServiceCaller(
        AppCaches appCaches,
        ITokenFactory tokenFactory,
        ITokenStorage tokenStorage,
        IKeyStorage keyStorage,
        IAuthorizationRequestSender authorizationRequestSender,
        ILogger<AuthorizedServiceCaller> logger,
        IOptionsMonitor<ServiceDetail> serviceDetailOptions,
        IHttpClientFactory httpClientFactory,
        JsonSerializerFactory jsonSerializerFactory,
        IAuthorizedRequestBuilder authorizedRequestBuilder,
        IRefreshTokenParametersBuilder refreshTokenParametersBuilder,
        IExchangeTokenParametersBuilder exchangeTokenParametersBuilder)
        : base(appCaches, tokenFactory, tokenStorage, keyStorage, authorizationRequestSender, logger, serviceDetailOptions)
    {
        _httpClientFactory = httpClientFactory;
        _jsonSerializerFactory = jsonSerializerFactory;
        _authorizedRequestBuilder = authorizedRequestBuilder;
        _refreshTokenParametersBuilder = refreshTokenParametersBuilder;
        _exchangeTokenParametersBuilder = exchangeTokenParametersBuilder;
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
        else
        {
            Token? token = GetAccessToken(serviceAlias) ?? throw new AuthorizedServiceException($"Cannot request service '{serviceAlias}' as access has not yet been authorized (no access token is available).");

            token = serviceDetail.CanExchangeToken
                ? await EnsureExchangeAccessToken(serviceAlias, token)
                : await EnsureAccessToken(serviceAlias, token);

            requestMessage = _authorizedRequestBuilder.CreateRequestMessageWithToken(serviceDetail, path, httpMethod, token, requestContent);
        }

        HttpResponseMessage response = await httpClient.SendAsync(requestMessage);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadAsStringAsync();
        }
        else if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return string.Empty;
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

    public string? GetToken(string serviceAlias)
    {
        Token? token = GetAccessToken(serviceAlias);
        return token?.AccessToken;
    }

    private async Task<Token> EnsureAccessToken(string serviceAlias, Token token)
    {
        ServiceDetail serviceDetail = GetServiceDetail(serviceAlias);
        if (token.WillBeExpiredAfter(serviceDetail.RefreshAccessTokenWhenExpiresWithin))
        {
            if (string.IsNullOrEmpty(token.RefreshToken))
            {
                ClearAccessToken(serviceAlias);
                throw new AuthorizedServiceException($"Cannot request service '{serviceAlias}' as the access token has or will expire and no refresh token is available to use. The expired token has been deleted.");
            }

            return await RefreshAccessToken(serviceAlias, token.RefreshToken)
                ?? throw new AuthorizedServiceException($"Cannot request service '{serviceAlias}' as the access token has or will expired and the refresh token could not be used to obtain a new access token.");
        }

        return token;
    }

    private async Task<Token?> RefreshAccessToken(string serviceAlias, string refreshToken)
    {
        ServiceDetail serviceDetail = GetServiceDetail(serviceAlias);

        Dictionary<string, string> parameters = _refreshTokenParametersBuilder.BuildParameters(serviceDetail, refreshToken);

        HttpResponseMessage response = await AuthorizationRequestSender.SendRequest(serviceDetail, parameters);
        if (response.IsSuccessStatusCode)
        {
            Token token = await CreateTokenFromResponse(serviceDetail, response);
            StoreToken(serviceAlias, token);
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

    private async Task<Token> EnsureExchangeAccessToken(string serviceAlias, Token token)
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

    private async Task<Token?> RefreshExchangeAccessToken(string serviceAlias, string accessToken)
    {
        ServiceDetail serviceDetail = GetServiceDetail(serviceAlias);

        Dictionary<string, string> parameters = _exchangeTokenParametersBuilder.BuildParameters(serviceDetail, accessToken);

        HttpResponseMessage response = await AuthorizationRequestSender.SendExchangeRequest(serviceDetail, parameters);
        if (response.IsSuccessStatusCode)
        {
            Token token = await CreateTokenFromResponse(serviceDetail, response);
            StoreToken(serviceAlias, token);
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

    private Token? GetAccessToken(string serviceAlias)
    {
        // First look in cache.
        var cacheKey = GetTokenCacheKey(serviceAlias);
        Token? token = AppCaches.RuntimeCache.GetCacheItem<Token>(cacheKey);
        if (token != null)
        {
            return token;
        }

        // Second, look in storage, and if found, save to cache.
        token = TokenStorage.GetToken(serviceAlias);
        if (token != null)
        {
            AppCaches.RuntimeCache.InsertCacheItem(cacheKey, () => token);
            return token;
        }

        return null;
    }

    private void ClearAccessToken(string serviceAlias)
    {
        AppCaches.RuntimeCache.ClearByKey(GetTokenCacheKey(serviceAlias));
        TokenStorage.DeleteToken(serviceAlias);
    }

    private static string GetTokenCacheKey(string serviceAlias) => $"Umbraco_AuthorizedServiceToken_{serviceAlias}";

    private class EmptyResponse
    {
    }
}
