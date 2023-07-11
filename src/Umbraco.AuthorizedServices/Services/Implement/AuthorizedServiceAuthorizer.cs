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

    public async Task<AuthorizationResult> AuthorizeServiceAsync(string serviceAlias, string authorizationCode, string redirectUri, string codeVerifier)
    {
        ServiceDetail serviceDetail = GetServiceDetail(serviceAlias);

        Dictionary<string, string> parameters = _authorizationParametersBuilder.BuildParameters(serviceDetail, authorizationCode, redirectUri, codeVerifier);

        HttpResponseMessage response = await AuthorizationRequestSender.SendRequest(serviceDetail, parameters);
        if (response.IsSuccessStatusCode)
        {
            Token token = await CreateTokenFromResponse(serviceDetail, response);

            StoreToken(serviceAlias, token);

            return AuthorizationResult.AsSuccess();
        }
        else
        {
            throw new AuthorizedServiceHttpException(
                $"Error response from token request to '{serviceAlias}'.",
                response.StatusCode,
                response.ReasonPhrase,
                await response.Content.ReadAsStringAsync());
        }
    }
}
