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
    private readonly IJsonSerializer _jsonSerializer;
    private readonly IAuthorizedRequestBuilder _authorizedRequestBuilder;
    private readonly IRefreshTokenParametersBuilder _refreshTokenParametersBuilder;

    public AuthorizedServiceCaller(
        AppCaches appCaches,
        ITokenFactory tokenFactory,
        ITokenStorage tokenStorage,
        IAuthorizationRequestSender authorizationRequestSender,
        ILogger<AuthorizedServiceCaller> logger,
        IOptionsMonitor<AuthorizedServiceSettings> authorizedServiceSettings,
        IHttpClientFactory httpClientFactory,
        IJsonSerializer jsonSerializer,
        IAuthorizedRequestBuilder authorizedRequestBuilder,
        IRefreshTokenParametersBuilder refreshTokenParametersBuilder)
        : base(appCaches, tokenFactory, tokenStorage, authorizationRequestSender, logger, authorizedServiceSettings.CurrentValue)
    {
        _httpClientFactory = httpClientFactory;
        _jsonSerializer = jsonSerializer;
        _authorizedRequestBuilder = authorizedRequestBuilder;
        _refreshTokenParametersBuilder = refreshTokenParametersBuilder;
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
            TResponse? result = _jsonSerializer.Deserialize<TResponse>(responseContent);
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

        Token? token = GetAccessToken(serviceAlias);
        if (token == null)
        {
            throw new AuthorizedServiceException($"Cannot request service '{serviceAlias}' as access has not yet been authorized.");
        }

        token = await EnsureAccessToken(serviceAlias, token);

        HttpClient httpClient = _httpClientFactory.CreateClient();

        HttpRequestMessage requestMessage = _authorizedRequestBuilder.CreateRequestMessage(serviceDetail, path, httpMethod, token, requestContent);

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

    private async Task<Token> EnsureAccessToken(string serviceAlias, Token token)
    {
        if (token.HasOrIsAboutToExpire)
        {
            if (string.IsNullOrEmpty(token.RefreshToken))
            {
                ClearAccessToken(serviceAlias);
                throw new AuthorizedServiceException($"Cannot request service '{serviceAlias}' as the access token has expired and no refresh token is available to use. The expired token has been deleted.");
            }

            Token? refreshedToken = await RefreshAccessToken(serviceAlias, token.RefreshToken);

            if (refreshedToken == null)
            {
                throw new AuthorizedServiceException($"Cannot request service '{serviceAlias}' as the access token has expired and the refresh token could not be used to obtain a new access token.");
            }

            return refreshedToken;
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
