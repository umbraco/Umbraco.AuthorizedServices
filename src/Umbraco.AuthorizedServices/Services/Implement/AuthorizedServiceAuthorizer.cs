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

    public AuthorizedServiceAuthorizer(
        AppCaches appCaches,
        ITokenFactory tokenFactory,
        ITokenStorage tokenStorage,
        IAuthorizationRequestSender authorizationRequestSender,
        ILogger<AuthorizedServiceAuthorizer> logger,
        IOptionsMonitor<ServiceDetail> serviceDetailOptions,
        IAuthorizationParametersBuilder authorizationParametersBuilder)
        : base(appCaches, tokenFactory, tokenStorage, authorizationRequestSender, logger, serviceDetailOptions)
    {
        _authorizationParametersBuilder = authorizationParametersBuilder;
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

    private async Task<AuthorizationResult> SendRequest(ServiceDetail serviceDetail, Dictionary<string, string> parameters)
    {
        HttpResponseMessage response = await AuthorizationRequestSender.SendRequest(serviceDetail, parameters);
        if (response.IsSuccessStatusCode)
        {
            Token token = await CreateTokenFromResponse(serviceDetail, response);

            StoreToken(serviceDetail.Alias, token);

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
