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
    private readonly IAuthorizedRequestBuilder _authorizedRequestBuilder;
    private readonly IRefreshTokenParametersBuilder _refreshTokenParametersBuilder;
    private readonly IExchangeTokenParametersBuilder _exchangeTokenParametersBuilder;
    private readonly IAuthorizedServiceAuthorizer _authorizedServiceAuthorizer;
    private readonly IServiceResponseMetadataParser _serviceResponseMetadataParser;

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
        IAuthorizedRequestBuilder authorizedRequestBuilder,
        IRefreshTokenParametersBuilder refreshTokenParametersBuilder,
        IExchangeTokenParametersBuilder exchangeTokenParametersBuilder,
        IAuthorizedServiceAuthorizer authorizedServiceAuthorizer,
        IServiceResponseMetadataParser serviceResponseMetadataParser)
        : base(appCaches, tokenFactory, oauth2TokenStorage, oauth1TokenStorage, keyStorage, authorizationRequestSender, logger, serviceDetailOptions)
    {
        _httpClientFactory = httpClientFactory;
        _jsonSerializerFactory = jsonSerializerFactory;
        _authorizedRequestBuilder = authorizedRequestBuilder;
        _refreshTokenParametersBuilder = refreshTokenParametersBuilder;
        _exchangeTokenParametersBuilder = exchangeTokenParametersBuilder;
        _authorizedServiceAuthorizer = authorizedServiceAuthorizer;
        _serviceResponseMetadataParser = serviceResponseMetadataParser;
    }

    public async Task<Attempt<AuthorizedServiceResponse<EmptyResponse>>> SendRequestAsync(string serviceAlias, string path, HttpMethod httpMethod)
      => await SendRequestAsync<object, EmptyResponse>(serviceAlias, path, httpMethod, null);

    public async Task<Attempt<AuthorizedServiceResponse<TResponse>>> SendRequestAsync<TResponse>(string serviceAlias, string path, HttpMethod httpMethod)
      => await SendRequestAsync<object, TResponse>(serviceAlias, path, httpMethod, null);

    public async Task<Attempt<AuthorizedServiceResponse<TResponse>>> SendRequestAsync<TRequest, TResponse>(string serviceAlias, string path, HttpMethod httpMethod, TRequest? requestContent = null)
        where TRequest : class
    {
        Attempt<AuthorizedServiceResponse<string>> responseContentAttempt = await SendRequestRawAsync(serviceAlias, path, httpMethod, requestContent);

        ServiceResponseMetadata serviceMetadata = responseContentAttempt.Result?.Metadata ?? new ServiceResponseMetadata();
        IDictionary<string, IEnumerable<string>> rawHeaders = responseContentAttempt.Result?.RawHeaders ?? new Dictionary<string, IEnumerable<string>>();

        if (!responseContentAttempt.Success)
        {
            if (responseContentAttempt.Exception is null)
            {
                return Attempt.Fail<AuthorizedServiceResponse<TResponse>>();
            }

            return Attempt.Fail(
                new AuthorizedServiceResponse<TResponse>(serviceMetadata, rawHeaders),
                responseContentAttempt.Exception)!;
        }

        var responseContent = responseContentAttempt.Result?.Data;
        if (!string.IsNullOrWhiteSpace(responseContent))
        {
            IJsonSerializer jsonSerializer = _jsonSerializerFactory.GetSerializer(serviceAlias);
            TResponse? result = jsonSerializer.Deserialize<TResponse>(responseContent);
            if (result != null)
            {
                return Attempt.Succeed(new AuthorizedServiceResponse<TResponse>(result, responseContent, serviceMetadata, rawHeaders))!;
            }

            return Attempt.Fail(
                new AuthorizedServiceResponse<TResponse>(serviceMetadata, rawHeaders),
                new AuthorizedServiceException($"Could not deserialize result of request to service '{serviceAlias}'"))!;
        }

        return Attempt.Succeed(new AuthorizedServiceResponse<TResponse>(serviceMetadata, rawHeaders))!;
    }

    public async Task<Attempt<AuthorizedServiceResponse<string>>> SendRequestRawAsync(string serviceAlias, string path, HttpMethod httpMethod)
      => await SendRequestRawAsync<object>(serviceAlias, path, httpMethod, null);

    public async Task<Attempt<AuthorizedServiceResponse<string>>> SendRequestRawAsync<TRequest>(string serviceAlias, string path, HttpMethod httpMethod, TRequest? requestContent = null)
        where TRequest : class
    {
        ServiceDetail serviceDetail = GetServiceDetail(serviceAlias);

        HttpClient httpClient = _httpClientFactory.CreateClient();

        Attempt<HttpRequestMessage?> requestMessageAttempt = await CreateHttpRequestMessage(serviceDetail, path, httpMethod, requestContent);
        if (!requestMessageAttempt.Success)
        {
            return Attempt.Fail(
                new AuthorizedServiceResponse<string>(),
                requestMessageAttempt.Exception!)!;
        }

        HttpResponseMessage response = await httpClient.SendAsync(requestMessageAttempt.Result!);

        ServiceResponseMetadata serviceMetadata = _serviceResponseMetadataParser.ParseMetadata(response);
        var rawHeaders = response.Headers
            .Union(response.Content.Headers)
            .ToDictionary(x => x.Key, x => x.Value);

        if (response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            return Attempt.Succeed(new AuthorizedServiceResponse<string>(responseContent, responseContent, serviceMetadata, rawHeaders))!;
        }

        return Attempt.Fail(
            new AuthorizedServiceResponse<string>(),
            new AuthorizedServiceHttpException(
                $"Error response received from request to '{serviceAlias}'.",
                response.StatusCode,
                response.ReasonPhrase,
                await response.Content.ReadAsStringAsync()))!;
    }

    private async Task<Attempt<HttpRequestMessage?>> CreateHttpRequestMessage<TRequest>(ServiceDetail serviceDetail, string path, HttpMethod httpMethod, TRequest? requestContent) where TRequest : class
    {
        switch (serviceDetail.AuthenticationMethod)
        {
            case AuthenticationMethod.ApiKey:
                return await CreateHttpRequestMessageForApiKeyAuthentication(serviceDetail, path, httpMethod, requestContent);
            case AuthenticationMethod.OAuth1:
                return await CreateHttpRequestMessageForOAuth1Authentication(serviceDetail, path, httpMethod, requestContent);
            case AuthenticationMethod.OAuth2AuthorizationCode:
            case AuthenticationMethod.OAuth2ClientCredentials:
                return await CreateRequestMessageForOAuth2Authentication(serviceDetail, path, httpMethod, requestContent);
            default:
                throw new ArgumentOutOfRangeException(nameof(serviceDetail.AuthenticationMethod));
        }
    }

    private async Task<Attempt<HttpRequestMessage?>> CreateHttpRequestMessageForApiKeyAuthentication<TRequest>(ServiceDetail serviceDetail, string path, HttpMethod httpMethod, TRequest? requestContent) where TRequest : class
    {
        var apiKey = !string.IsNullOrEmpty(serviceDetail.ApiKey)
            ? serviceDetail.ApiKey
            : await GetApiKeyFromCacheOrStorage(serviceDetail.Alias);

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return Attempt.Fail<HttpRequestMessage?>(
                default,
                new AuthorizedServiceException($"Cannot request service '{serviceDetail.Alias}' as access has not yet been authorized (no API key is configured or stored)."));
        }

        HttpRequestMessage requestMessage = _authorizedRequestBuilder.CreateRequestMessageWithApiKey(
            serviceDetail,
            path,
            httpMethod,
            apiKey,
            requestContent);
        return Attempt.Succeed(requestMessage);
    }

    private async Task<Attempt<HttpRequestMessage?>> CreateHttpRequestMessageForOAuth1Authentication<TRequest>(ServiceDetail serviceDetail, string path, HttpMethod httpMethod, TRequest? requestContent) where TRequest : class
    {
        OAuth1Token? token = await GetOAuth1TokenFromCacheOrStorage(serviceDetail.Alias);
        if (token is null)
        {
            return Attempt.Fail<HttpRequestMessage?>(
                default,
                new AuthorizedServiceException($"Cannot request service '{serviceDetail.Alias}' as access has not yet been authorized (no OAuth token is available)."));
        }

        HttpRequestMessage requestMessage = _authorizedRequestBuilder.CreateRequestMessageWithOAuth1Token(serviceDetail, path, httpMethod, token, requestContent);
        return Attempt.Succeed(requestMessage);
    }

    private async Task<Attempt<HttpRequestMessage?>> CreateRequestMessageForOAuth2Authentication<TRequest>(ServiceDetail serviceDetail, string path, HttpMethod httpMethod, TRequest? requestContent) where TRequest : class
    {
        OAuth2Token? token = await GetOAuth2TokenFromCacheOrStorage(serviceDetail.Alias);
        if (token is null)
        {
            return Attempt.Fail<HttpRequestMessage?>(
                default,
                new AuthorizedServiceException($"Cannot request service '{serviceDetail.Alias}' as access has not yet been authorized (no access token is available)."));
        }

        token = serviceDetail.CanExchangeToken
            ? await EnsureExchangeAccessToken(serviceDetail, token)
            : await EnsureAccessToken(serviceDetail, token);

        HttpRequestMessage requestMessage = _authorizedRequestBuilder.CreateRequestMessageWithOAuth2Token(serviceDetail, path, httpMethod, token, requestContent);
        return Attempt.Succeed(requestMessage);
    }

    public async Task<Attempt<string?>> GetApiKey(string serviceAlias)
    {
        // First check for configured API key.
        var apiKey = GetServiceDetail(serviceAlias)?.ApiKey;
        if (!string.IsNullOrWhiteSpace(apiKey))
        {
            return Attempt.Succeed(apiKey);
        }

        // If not found, look for stored key.
        apiKey = await KeyStorage.GetKeyAsync(serviceAlias);
        if (!string.IsNullOrWhiteSpace(apiKey))
        {
            return Attempt.Succeed(apiKey);
        }

        return Attempt.Fail<string?>();
    }

    public async Task<Attempt<string?>> GetOAuth2AccessToken(string serviceAlias)
    {
        var accessToken = (await GetOAuth2TokenFromCacheOrStorage(serviceAlias))?.AccessToken;
        if (!string.IsNullOrWhiteSpace(accessToken))
        {
            return Attempt.Succeed(accessToken);
        }

        return Attempt.Fail<string?>();
    }

    public async Task<Attempt<string?>> GetOAuth1Token(string serviceAlias)
    {
        var token = (await GetOAuth1TokenFromCacheOrStorage(serviceAlias))?.OAuthToken;
        if (!string.IsNullOrWhiteSpace(token))
        {
            return Attempt.Succeed(token);
        }

        return Attempt.Fail<string?>();
    }

    private async Task<OAuth2Token> EnsureAccessToken(ServiceDetail serviceDetail, OAuth2Token token)
    {
        if (token.WillBeExpiredAfter(serviceDetail.RefreshAccessTokenWhenExpiresWithin))
        {
            switch (serviceDetail.AuthenticationMethod)
            {
                case AuthenticationMethod.OAuth2AuthorizationCode:

                    // For OAuth2 authorization code flow we can get a new access token if we have a refresh token.
                    if (string.IsNullOrEmpty(token.RefreshToken))
                    {
                        await ClearOAuth2AccessToken(serviceDetail.Alias);
                        throw new AuthorizedServiceException($"Cannot request service '{serviceDetail.Alias}' as the access token has or will expire and no refresh token is available to use. The expired token has been deleted.");
                    }

                    return await RefreshAccessToken(serviceDetail, token.RefreshToken)
                        ?? throw new AuthorizedServiceException($"Cannot request service '{serviceDetail.Alias}' as the access token has or will expired and the refresh token could not be used to obtain a new access token.");
                case AuthenticationMethod.OAuth2ClientCredentials:

                    // For OAuth2 client credentials flow we can request a new access token via the same means as retrieving the original one.
                    AuthorizationResult result = await _authorizedServiceAuthorizer.AuthorizeOAuth2ClientCredentialsServiceAsync(serviceDetail.Alias);
                    if (result.Success)
                    {
                        OAuth2Token? newToken = await OAuth2TokenStorage.GetTokenAsync(serviceDetail.Alias);
                        if (newToken is not null)
                        {
                            return newToken;
                        }

                        throw new AuthorizedServiceException($"Cannot request service '{serviceDetail.Alias}' as the access token has or will expired and although the client credentials completed successfully a new access token could not be retrieved from storage.");
                    }

                    throw new AuthorizedServiceException($"Cannot request service '{serviceDetail.Alias}' as the access token has or will expired and the client credentials flow did not complete successfully in obtaining a new access token. {result.ErrorMessage}.");
                default:
                    throw new ArgumentException($"{serviceDetail.AuthenticationMethod} is not expected here. Only OAuth2 access tokens can be refreshed or renewed.");
            }
        }

        return token;
    }

    private async Task<OAuth2Token?> RefreshAccessToken(ServiceDetail serviceDetail, string refreshToken)
    {
        Dictionary<string, string> parameters = _refreshTokenParametersBuilder.BuildParameters(serviceDetail, refreshToken);

        HttpResponseMessage response = await AuthorizationRequestSender.SendOAuth2Request(serviceDetail, parameters);
        if (response.IsSuccessStatusCode)
        {
            OAuth2Token token = await CreateOAuth2TokenFromResponse(serviceDetail, response);
            await StoreOAuth2Token(serviceDetail.Alias, token);
            return token;
        }
        else
        {
            throw new AuthorizedServiceHttpException(
                $"Error response from refresh token request to '{serviceDetail.Alias}'.",
                response.StatusCode,
                response.ReasonPhrase,
                await response.Content.ReadAsStringAsync());
        }
    }

    private async Task<OAuth2Token> EnsureExchangeAccessToken(ServiceDetail serviceDetail, OAuth2Token token)
    {
        if (serviceDetail.ExchangeTokenProvision is not null
            && token.WillBeExpiredAfter(serviceDetail.ExchangeTokenProvision.ExchangeTokenWhenExpiresWithin))
        {
            return await RefreshExchangeAccessToken(serviceDetail.Alias, token.AccessToken)
                ?? throw new AuthorizedServiceException($"Cannot request service '{serviceDetail.Alias}' as the access token will expire and it's not been possible to exchange it for a new access token.");
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
            await StoreOAuth2Token(serviceAlias, token);
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

    private async Task<string?> GetApiKeyFromCacheOrStorage(string serviceAlias)
    {
        // First look in cache.
        var cacheKey = CacheHelper.GetApiKeyCacheKey(serviceAlias);
        string? apiKey = AppCaches.RuntimeCache.GetCacheItem<string>(cacheKey);
        if (!string.IsNullOrWhiteSpace(apiKey))
        {
            return apiKey;
        }

        // Second, look in storage, and if found, save to cache.
        apiKey = await KeyStorage.GetKeyAsync(serviceAlias);
        if (!string.IsNullOrWhiteSpace(apiKey))
        {
            AppCaches.RuntimeCache.InsertCacheItem(cacheKey, () => apiKey);
            return apiKey;
        }

        return null;
    }

    private async Task<OAuth2Token?> GetOAuth2TokenFromCacheOrStorage(string serviceAlias)
    {
        // First look in cache.
        var cacheKey = CacheHelper.GetTokenCacheKey(serviceAlias);
        OAuth2Token? token = AppCaches.RuntimeCache.GetCacheItem<OAuth2Token>(cacheKey);
        if (token != null)
        {
            return token;
        }

        // Second, look in storage, and if found, save to cache.
        token = await OAuth2TokenStorage.GetTokenAsync(serviceAlias);
        if (token != null)
        {
            AppCaches.RuntimeCache.InsertCacheItem(cacheKey, () => token);
            return token;
        }

        return null;
    }

    private async Task<OAuth1Token?> GetOAuth1TokenFromCacheOrStorage(string serviceAlias)
    {
        // First look in cache.
        var cacheKey = CacheHelper.GetTokenCacheKey(serviceAlias);
        OAuth1Token? token = AppCaches.RuntimeCache.GetCacheItem<OAuth1Token>(cacheKey);
        if (token != null)
        {
            return token;
        }

        // Second, look in storage, and if found, save to cache.
        token = await OAuth1TokenStorage.GetTokenAsync(serviceAlias);
        if (token != null)
        {
            AppCaches.RuntimeCache.InsertCacheItem(cacheKey, () => token);
            return token;
        }

        return null;
    }

    private async Task ClearOAuth2AccessToken(string serviceAlias)
    {
        var cacheKey = CacheHelper.GetTokenCacheKey(serviceAlias);
        AppCaches.RuntimeCache.ClearByKey(cacheKey);
        await OAuth2TokenStorage.DeleteTokenAsync(serviceAlias);
    }
}
