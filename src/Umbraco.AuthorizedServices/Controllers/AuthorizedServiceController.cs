using System.IO;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Umbraco.AuthorizedServices.Configuration;
using Umbraco.AuthorizedServices.Exceptions;
using Umbraco.AuthorizedServices.Helpers;
using Umbraco.AuthorizedServices.Models;
using Umbraco.AuthorizedServices.Models.Request;
using Umbraco.AuthorizedServices.Services;
using Umbraco.Cms.Web.BackOffice.Controllers;
using Umbraco.Cms.Web.Common.Attributes;

namespace Umbraco.AuthorizedServices.Controllers;

/// <summary>
/// Backoffice controller used for working with authorized services.
/// </summary>
[PluginController(Constants.PluginName)]
[Authorize(Policy = Cms.Web.Common.Authorization.AuthorizationPolicies.SectionAccessSettings)]
public class AuthorizedServiceController : BackOfficeNotificationsController
{
    private readonly IOptionsMonitor<ServiceDetail> _serviceDetailOptions;
    private readonly ITokenStorage<Token> _tokenStorage;
    private readonly ITokenStorage<OAuth1aToken> _oauth1aTokenStorage;
    private readonly IKeyStorage _keyStorage;
    private readonly ITokenCache _tokenCache;
    private readonly IOAuth1aAuthorizationUrlBuilder _oauth1aAuthorizationUrlBuilder;
    private readonly IAuthorizationUrlBuilder _authorizationUrlBuilder;
    private readonly IAuthorizedServiceCaller _authorizedServiceCaller;
    private readonly IAuthorizationPayloadCache _authorizedServiceAuthorizationPayloadCache;
    private readonly IAuthorizationPayloadBuilder _authorizedServiceAuthorizationPayloadBuilder;
    private readonly IAuthorizedServiceAuthorizer _serviceAuthorizer;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthorizedServiceController"/> class.
    /// </summary>
    public AuthorizedServiceController(
        IOptionsMonitor<ServiceDetail> serviceDetailOptions,
        ITokenStorage<Token> tokenStorage,
        ITokenStorage<OAuth1aToken> oauth1aTokenStorage,
        IKeyStorage keyStorage,
        ITokenCache tokenCache,
        IOAuth1aAuthorizationUrlBuilder oauth1aAuthorizationUrlBuilder,
        IAuthorizationUrlBuilder authorizationUrlBuilder,
        IAuthorizedServiceCaller authorizedServiceCaller,
        IAuthorizationPayloadCache authorizedServiceAuthorizationPayloadCache,
        IAuthorizationPayloadBuilder authorizedServiceAuthorizationPayloadBuilder,
        IAuthorizedServiceAuthorizer serviceAuthorizer)
    {
        _serviceDetailOptions = serviceDetailOptions;
        _tokenStorage = tokenStorage;
        _oauth1aTokenStorage = oauth1aTokenStorage;
        _keyStorage = keyStorage;
        _tokenCache = tokenCache;
        _oauth1aAuthorizationUrlBuilder = oauth1aAuthorizationUrlBuilder;
        _authorizationUrlBuilder = authorizationUrlBuilder;
        _authorizedServiceCaller = authorizedServiceCaller;
        _authorizedServiceAuthorizationPayloadCache = authorizedServiceAuthorizationPayloadCache;
        _authorizedServiceAuthorizationPayloadBuilder = authorizedServiceAuthorizationPayloadBuilder;
        _serviceAuthorizer = serviceAuthorizer;
    }

    /// <summary>
    /// Retrieves the details for a service by alias.
    /// </summary>
    /// <param name="alias">The service alias.</param>
    [HttpGet]
    public async Task<AuthorizedServiceDisplay?> GetByAlias(string alias)
    {
        ServiceDetail serviceDetail = _serviceDetailOptions.Get(alias);

        bool isAuthorized = CheckAuthorizationStatus(serviceDetail);

        string? authorizationUrl = null;
        if (serviceDetail.AuthenticationMethod == AuthenticationMethod.OAuth2AuthorizationCode)
        {
            if (!isAuthorized)
            {
                AuthorizationPayload authorizationPayload = _authorizedServiceAuthorizationPayloadBuilder.BuildPayload();

                _authorizedServiceAuthorizationPayloadCache.Add(alias, authorizationPayload);

                authorizationUrl = _authorizationUrlBuilder
                    .BuildUrl(serviceDetail, HttpContext, authorizationPayload.State, authorizationPayload.CodeChallenge);
            }
        }

        if (serviceDetail.AuthenticationMethod == AuthenticationMethod.OAuth1 && !isAuthorized)
        {
            var response = await _authorizedServiceCaller.SendRequestRawAsync(alias, serviceDetail.RequestAuthorizationPath, HttpMethod.Post);

            if (response is not null && response.TryParseOAuth1aResponse(out var oauthToken, out var oauthTokenSecret))
            {
                _tokenCache.Save(serviceDetail.Alias, oauthToken);

                authorizationUrl = _oauth1aAuthorizationUrlBuilder.BuildAuthorizationUrl(serviceDetail, response);
            }
        }

        return new AuthorizedServiceDisplay
        {
            DisplayName = serviceDetail.DisplayName,
            IsAuthorized = isAuthorized,
            CanManuallyProvideToken = serviceDetail.CanManuallyProvideToken,
            CanManuallyProvideApiKey = serviceDetail.CanManuallyProvideApiKey,
            AuthorizationUrl = authorizationUrl,
            AuthenticationMethod = serviceDetail.AuthenticationMethod.ToString(),
            SampleRequest = serviceDetail.SampleRequest,
            Settings = new Dictionary<string, string>
            {
                { nameof(ServiceDetail.Alias), serviceDetail.Alias },
                { nameof(ServiceDetail.DisplayName), serviceDetail.DisplayName },
                { nameof(ServiceDetail.ApiHost), serviceDetail.ApiHost },
                { nameof(ServiceDetail.AuthenticationMethod), serviceDetail.AuthenticationMethod.ToString() },
                { nameof(ServiceDetail.IdentityHost), serviceDetail.IdentityHost },
                { nameof(ServiceDetail.TokenHost), serviceDetail.TokenHost },
                { nameof(ServiceDetail.RequestIdentityPath), serviceDetail.RequestIdentityPath },
                { nameof(ServiceDetail.RequestTokenPath), serviceDetail.RequestTokenPath },
                { nameof(ServiceDetail.RequestTokenFormat), serviceDetail.RequestTokenFormat is not null ? serviceDetail.RequestTokenFormat.Value.ToString() : string.Empty },
                { nameof(ServiceDetail.ApiKey), serviceDetail.ApiKey },
                { nameof(ServiceDetail.ApiKeyProvision), serviceDetail.ApiKeyProvision is not null ? serviceDetail.ApiKeyProvision.ToString() : string.Empty },
                { nameof(ServiceDetail.ClientId), serviceDetail.ClientId },
                { nameof(ServiceDetail.ClientSecret), new string('*', serviceDetail.ClientSecret.Length) },
                { nameof(ServiceDetail.Scopes), serviceDetail.Scopes },
                {
                    nameof(ServiceDetail.SampleRequest),
                    !string.IsNullOrEmpty(serviceDetail.SampleRequest)
                        ? serviceDetail.SampleRequest
                        : string.Empty
                }
            }
        };
    }

    /// <summary>
    /// Sends a sample request for an authorized service.
    /// </summary>
    /// <param name="alias">The service alias.</param>
    /// <param name="path">The path to the sample request.</param>
    /// <returns></returns>
    [HttpGet]
    public async Task<IActionResult> SendSampleRequest(string alias, string path)
    {
        string response = await _authorizedServiceCaller.SendRequestRawAsync(alias, path, HttpMethod.Get);
        return Ok(response);
    }

    /// <summary>
    /// Revokes access by removing the access token or API key for an authorized service.
    /// </summary>
    /// <param name="model">Request model identifying the service.</param>
    [HttpPost]
    public IActionResult RevokeAccess(RevokeAccess model)
    {
        ServiceDetail serviceDetail = _serviceDetailOptions.Get(model.Alias);
        if (serviceDetail.AuthenticationMethod != AuthenticationMethod.ApiKey)
        {
            _tokenStorage.DeleteToken(model.Alias);
        }
        else
        {
            _keyStorage.DeleteKey(model.Alias);
        }
        return Ok();
    }

    /// <summary>
    /// Adds a new access token for an authorized service.
    /// </summary>
    /// <param name="model">Request model identifying the service.</param>
    /// <returns></returns>
    [HttpPost]
    public IActionResult SaveToken(AddToken model)
    {
        _tokenStorage.SaveToken(model.Alias, new Token(model.Token));
        return Ok();
    }

    /// <summary>
    /// Adds a new API key for an authorized service.
    /// </summary>
    /// <param name="model">Request model identifying the service.</param>
    /// <returns></returns>
    [HttpPost]
    public IActionResult SaveApiKey(AddApiKey model)
    {
        _keyStorage.SaveKey(model.Alias, model.ApiKey);
        return Ok();
    }

    /// <summary>
    /// Generates access token for an authorized service.
    /// </summary>
    /// <param name="model">Request model identifying the service.</param>
    /// <returns></returns>
    [HttpPost]
    public async Task<IActionResult> GenerateToken(GenerateToken model)
    {
        ServiceDetail serviceDetail = _serviceDetailOptions.Get(model.Alias);

        Models.AuthorizationResult result = await _serviceAuthorizer
            .AuthorizeOAuth2ClientCredentialsServiceAsync(serviceDetail.Alias);

        if (result.Success)
        {
            return Ok(result);
        }

        throw new AuthorizedServiceException("Failed to obtain access token");
    }

    private bool CheckAuthorizationStatus(ServiceDetail serviceDetail) => serviceDetail.AuthenticationMethod switch
    {
        AuthenticationMethod.OAuth1 => StoredOAuth1aTokenExists(serviceDetail),
        AuthenticationMethod.OAuth2AuthorizationCode => StoredTokenExists(serviceDetail),
        AuthenticationMethod.OAuth2ClientCredentials => StoredTokenExists(serviceDetail),
        AuthenticationMethod.ApiKey => !string.IsNullOrEmpty(serviceDetail.ApiKey)
                                        || _keyStorage.GetKey(serviceDetail.Alias) is not null,
        _ => false
    };

    private bool StoredTokenExists(ServiceDetail serviceDetail) => _tokenStorage.GetToken(serviceDetail.Alias) != null;

    private bool StoredOAuth1aTokenExists(ServiceDetail serviceDetail) => _oauth1aTokenStorage.GetToken(serviceDetail.Alias) != null;
}
