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
        IOAuth2TokenStorage oauth2TokenStorage,
        IOAuth1TokenStorage oauth1TokenStorage,
        IKeyStorage keyStorage,
        IAuthorizationRequestSender authorizationRequestSender,
        ILogger<AuthorizedServiceAuthorizer> logger,
        IOptionsMonitor<ServiceDetail> serviceDetailOptions,
        IAuthorizationParametersBuilder authorizationParametersBuilder,
        IExchangeTokenParametersBuilder exchangeTokenParametersBuilder)
        : base(appCaches, tokenFactory, oauth2TokenStorage, oauth1TokenStorage, keyStorage, authorizationRequestSender, logger, serviceDetailOptions)
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

        OAuth2Token? token = GetStoredToken(serviceAlias) ?? throw new AuthorizedServiceException($"Could not find the access token for {serviceAlias}");

        Dictionary<string, string> parameters = _exchangeTokenParametersBuilder.BuildParameters(serviceDetail, token.AccessToken);

        return await SendRequest(serviceDetail, parameters, true);
    }

    public async Task<AuthorizationResult> AuthorizeOAuth1ServiceAsync(string serviceAlias, string oauthToken, string oauthVerifier)
    {
        ServiceDetail serviceDetail = GetServiceDetail(serviceAlias);

        return await SendRequest(serviceDetail, new Dictionary<string, string>
        {
            { Constants.OAuth1.OAuthToken , oauthToken },
            { Constants.OAuth1.OAuthVerifier, oauthVerifier }
        });
    }

    private async Task<AuthorizationResult> SendRequest(ServiceDetail serviceDetail, Dictionary<string, string> parameters, bool isExchangeTokenRequest = false)
    {
        HttpResponseMessage response = serviceDetail.AuthenticationMethod == AuthenticationMethod.OAuth1
            ? await AuthorizationRequestSender.SendOAuth1Request(serviceDetail, parameters)
            : (isExchangeTokenRequest
                    ? await AuthorizationRequestSender.SendOAuth2ExchangeRequest(serviceDetail, parameters)
                    : await AuthorizationRequestSender.SendOAuth2Request(serviceDetail, parameters));
        if (response.IsSuccessStatusCode)
        {
            if (serviceDetail.AuthenticationMethod == AuthenticationMethod.OAuth1)
            {
                OAuth1Token token = await CreateOAuth1TokenFromResponse(response);
                StoreOAuth1Token(serviceDetail.Alias, token);
            }
            else
            {
                OAuth2Token token = await CreateOAuth2TokenFromResponse(serviceDetail, response);
                StoreOAuth2Token(serviceDetail.Alias, token);
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
