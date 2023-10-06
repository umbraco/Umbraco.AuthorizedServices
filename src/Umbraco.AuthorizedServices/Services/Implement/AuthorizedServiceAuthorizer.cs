using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.AuthorizedServices.Configuration;
using Umbraco.AuthorizedServices.Exceptions;
using Umbraco.AuthorizedServices.Models;
using Umbraco.Cms.Core.Cache;

namespace Umbraco.AuthorizedServices.Services.Implement;

internal sealed class AuthorizedServiceAuthorizer : AuthorizedServiceBase, IAuthorizedServiceAuthorizer
{
    private readonly IAuthorizationParametersBuilder _authorizationParametersBuilder;
    private readonly IExchangeTokenParametersBuilder _exchangeTokenParametersBuilder;

    public AuthorizedServiceAuthorizer(
        AppCaches appCaches,
        ITokenFactory tokenFactory,
        ITokenStorage<Token> tokenStorage,
        ITokenStorage<OAuth1aToken> oauth1aTokenStorage,
        IKeyStorage keyStorage,
        IAuthorizationRequestSender authorizationRequestSender,
        ILogger<AuthorizedServiceAuthorizer> logger,
        IOptionsMonitor<ServiceDetail> serviceDetailOptions,
        IAuthorizationParametersBuilder authorizationParametersBuilder,
        IExchangeTokenParametersBuilder exchangeTokenParametersBuilder)
        : base(appCaches, tokenFactory, tokenStorage, oauth1aTokenStorage, keyStorage, authorizationRequestSender, logger, serviceDetailOptions)
    {
        _authorizationParametersBuilder = authorizationParametersBuilder;
        _exchangeTokenParametersBuilder = exchangeTokenParametersBuilder;
    }

    public async Task<AuthorizationResult> AuthorizeOAuth2AuthorizationCodeServiceAsync(string serviceAlias, string authorizationCode, string redirectUri, string codeVerifier)
    {
        ServiceDetail serviceDetail = GetServiceDetail(serviceAlias);

        Dictionary<string, string> parameters = _authorizationParametersBuilder.BuildParametersForOAuth2AuthorizationCode(serviceDetail, authorizationCode, redirectUri, codeVerifier);

        return await SendRequest(serviceDetail, parameters);
    }

    public async Task<AuthorizationResult> AuthorizeOAuth2ClientCredentialsServiceAsync(string serviceAlias)
    {
        ServiceDetail serviceDetail = GetServiceDetail(serviceAlias);

        Dictionary<string, string> parameters = _authorizationParametersBuilder.BuildParametersForOAuth2ClientCredentials(serviceDetail);

        return await SendRequest(serviceDetail, parameters);
    }

    public async Task<AuthorizationResult> ExchangeOAuth2AccessTokenAsync(string serviceAlias)
    {
        ServiceDetail serviceDetail = GetServiceDetail(serviceAlias);

        Token? token = GetStoredToken(serviceAlias) ?? throw new AuthorizedServiceException($"Could not find the access token for {serviceAlias}");

        Dictionary<string, string> parameters = _exchangeTokenParametersBuilder.BuildParameters(serviceDetail, token.AccessToken);

        return await SendRequest(serviceDetail, parameters, true);
    }

    public async Task<AuthorizationResult> AuthorizeOAuth1ServiceAsync(string serviceAlias, string oauthToken, string oauthVerifier)
    {
        ServiceDetail serviceDetail = GetServiceDetail(serviceAlias);

        return await SendRequest(serviceDetail, new Dictionary<string, string>
        {
            { "oauth_token", oauthToken },
            { "oauth_verifier", oauthVerifier }
        });
    }

    private async Task<AuthorizationResult> SendRequest(ServiceDetail serviceDetail, Dictionary<string, string> parameters, bool isExchangeTokenRequest = false)
    {
        HttpResponseMessage response = serviceDetail.AuthenticationMethod == AuthenticationMethod.OAuth1
            ? await AuthorizationRequestSender.SendOAuth1aRequest(serviceDetail, parameters)
            : (isExchangeTokenRequest
                    ? await AuthorizationRequestSender.SendExchangeRequest(serviceDetail, parameters)
                    : await AuthorizationRequestSender.SendRequest(serviceDetail, parameters));
        if (response.IsSuccessStatusCode)
        {
            if (serviceDetail.AuthenticationMethod == AuthenticationMethod.OAuth1)
            {
                OAuth1aToken token = await CreateOAuth1aTokenFromResponse(serviceDetail, response);
                StoreOAuth1aToken(serviceDetail.Alias, token);
            }
            else
            {
                Token token = await CreateTokenFromResponse(serviceDetail, response);
                StoreToken(serviceDetail.Alias, token);
            }

            return AuthorizationResult.AsSuccess();
        }
        else
        {
            throw new AuthorizedServiceHttpException(
                $"Error response from token request to '{serviceDetail.Alias}'.",
                response.StatusCode,
                response.ReasonPhrase,
                await response.Content.ReadAsStringAsync());
        }
    }

}
