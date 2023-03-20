using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.AuthorizedServices.Configuration;
using Umbraco.AuthorizedServices.Models;
using Umbraco.Cms.Core.Cache;

namespace Umbraco.AuthorizedServices.Services.Implement;

internal sealed class AuthorizedServiceAuthorizer : AuthorizedServiceBase, IAuthorizedServiceAuthorizer
{
    private readonly IAuthorizationParametersBuilder _authorizationParametersBuilder;

    public AuthorizedServiceAuthorizer(
        IHttpClientFactory httpClientFactory,
        AppCaches appCaches,
        ITokenFactory tokenFactory,
        ITokenStorage tokenStorage,
        IAuthorizationRequestSender authorizationRequestSender,
        ILogger<AuthorizedServiceAuthorizer> logger,
        IOptionsMonitor<AuthorizedServiceSettings> authorizedServiceSettings,
        IAuthorizationParametersBuilder authorizationParametersBuilder)
        : base(httpClientFactory, appCaches, tokenFactory, tokenStorage, authorizationRequestSender, logger, authorizedServiceSettings.CurrentValue)
    {
        _authorizationParametersBuilder = authorizationParametersBuilder;
    }

    public async Task<AuthorizationResult> AuthorizeServiceAsync(string serviceAlias, string authorizationCode, string redirectUri)
    {
        ServiceDetail serviceDetail = GetServiceDetail(serviceAlias);

        Dictionary<string, string> parameters = _authorizationParametersBuilder.BuildParameters(serviceDetail, authorizationCode, redirectUri);

        HttpResponseMessage response = await AuthorizationRequestSender.SendRequest(serviceDetail, parameters);
        if (response.IsSuccessStatusCode)
        {
            Token token = await CreateTokenFromResponse(serviceAlias, serviceDetail, response);

            StoreToken(serviceAlias, token);

            return AuthorizationResult.AsSuccess();
        }
        else
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Error response from token request to '{serviceAlias}'. Response: {responseContent}");
        }
    }
}
