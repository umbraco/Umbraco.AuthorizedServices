using System.IO;
using System.Security.AccessControl;
using Azure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Umbraco.AuthorizedServices.Configuration;
using Umbraco.AuthorizedServices.Exceptions;
using Umbraco.AuthorizedServices.Helpers;
using Umbraco.AuthorizedServices.Models;
using Umbraco.AuthorizedServices.Models.Request;
using Umbraco.AuthorizedServices.Services;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Web.BackOffice.Controllers;
using Umbraco.Cms.Web.Common.Attributes;
using Umbraco.Extensions;

namespace Umbraco.AuthorizedServices.Controllers;

/// <summary>
/// Backoffice controller used for working with authorized services.
/// </summary>
[PluginController(Constants.PluginName)]
[Authorize(Policy = Cms.Web.Common.Authorization.AuthorizationPolicies.SectionAccessSettings)]
public class AuthorizedServiceController : BackOfficeNotificationsController
{
    private readonly IOptionsMonitor<ServiceDetail> _serviceDetailOptions;
    private readonly IOAuth2TokenStorage _oauth2TokenStorage;
    private readonly IOAuth1TokenStorage _oauth1TokenStorage;
    private readonly IKeyStorage _keyStorage;
    private readonly AppCaches _appCaches;
    private readonly IAuthorizationUrlBuilder _authorizationUrlBuilder;
    private readonly IAuthorizedServiceCaller _authorizedServiceCaller;
    private readonly IAuthorizationPayloadBuilder _authorizationPayloadBuilder;
    private readonly IAuthorizedServiceAuthorizer _serviceAuthorizer;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthorizedServiceController"/> class.
    /// </summary>
    public AuthorizedServiceController(
        IOptionsMonitor<ServiceDetail> serviceDetailOptions,
        IOAuth1TokenStorage oauth1TokenStorage,
        IOAuth2TokenStorage oauth2TokenStorage,
        IKeyStorage keyStorage,
        AppCaches appCaches,
        IAuthorizationUrlBuilder authorizationUrlBuilder,
        IAuthorizedServiceCaller authorizedServiceCaller,
        IAuthorizationPayloadBuilder authorizationPayloadBuilder,
        IAuthorizedServiceAuthorizer serviceAuthorizer)
    {
        _serviceDetailOptions = serviceDetailOptions;
        _oauth1TokenStorage = oauth1TokenStorage;
        _oauth2TokenStorage = oauth2TokenStorage;
        _keyStorage = keyStorage;
        _appCaches = appCaches;
        _authorizationUrlBuilder = authorizationUrlBuilder;
        _authorizedServiceCaller = authorizedServiceCaller;
        _authorizationPayloadBuilder = authorizationPayloadBuilder;
        _serviceAuthorizer = serviceAuthorizer;
    }

    /// <summary>
    /// Retrieves the details for a service by alias.
    /// </summary>
    /// <param name="alias">The service alias.</param>
    [HttpGet]
    public AuthorizedServiceDisplay? GetByAlias(string alias)
    {
        ServiceDetail serviceDetail = _serviceDetailOptions.Get(alias);

        bool isAuthorized = CheckAuthorizationStatus(serviceDetail);

        string? authorizationUrl = null;
        if (serviceDetail.AuthenticationMethod == AuthenticationMethod.OAuth2AuthorizationCode)
        {
            if (!isAuthorized)
            {
                AuthorizationPayload authorizationPayload = _authorizationPayloadBuilder.BuildPayload();

                var cacheKey = string.Format(Constants.Cache.AuthorizationPayloadKeyFormat, alias);
                _appCaches.RuntimeCache.Insert(cacheKey, () => authorizationPayload);

                authorizationUrl = _authorizationUrlBuilder
                    .BuildOAuth2AuthorizationUrl(serviceDetail, HttpContext, authorizationPayload.State, authorizationPayload.CodeChallenge);
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

        switch (serviceDetail.AuthenticationMethod)
        {
            case AuthenticationMethod.ApiKey:
                _keyStorage.DeleteKey(model.Alias);
                break;
            case AuthenticationMethod.OAuth1:
                _oauth1TokenStorage.DeleteToken(model.Alias);
                break;
            case AuthenticationMethod.OAuth2AuthorizationCode:
            case AuthenticationMethod.OAuth2ClientCredentials:
                _oauth2TokenStorage.DeleteToken(model.Alias);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(serviceDetail.AuthenticationMethod));
        }

        return Ok();
    }

    /// <summary>
    /// Adds a new access token for an OAuth2 authorized service.
    /// </summary>
    /// <param name="model">Request model identifying the service.</param>
    /// <returns></returns>
    [HttpPost]
    public IActionResult SaveOAuth2Token(AddOAuth2Token model)
    {
        _oauth2TokenStorage.SaveToken(model.Alias, new OAuth2Token(model.Token));
        return Ok();
    }

    /// <summary>
    /// Adds a new access token/token secret pair for an OAuth1 authorized service.
    /// </summary>
    /// <param name="model">Request model identifying the service.</param>
    /// <returns></returns>
    [HttpPost]
    public IActionResult SaveOAuth1Token(AddOAuth1Token model)
    {
        _oauth1TokenStorage.SaveToken(model.Alias, new OAuth1Token(model.Token, model.TokenSecret));
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
    /// Generates access token for an authorized service (using OAuth2).
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

    [HttpPost]
    public async Task<IActionResult> GenerateRequestToken(GenerateToken model)
    {
        ServiceDetail serviceDetail = _serviceDetailOptions.Get(model.Alias);

        var url = _authorizationUrlBuilder.BuildOAuth1RequestTokenUrl(serviceDetail, HttpContext, HttpMethod.Post, OAuth1Helper.GetNonce(), OAuth1Helper.GetTimestamp());

        Models.AuthorizationResult requestTokenResponse = await _serviceAuthorizer.GenerateOAuth1RequestTokenAsync(model.Alias, url);

        if (requestTokenResponse.Success && requestTokenResponse.Result is not null && requestTokenResponse.Result.TryParseOAuth1Response(out var oauthToken, out _))
        {
            _appCaches.RuntimeCache.InsertCacheItem(oauthToken, () => serviceDetail.Alias);

            return Ok(string.Format(
                "{0}{1}?{2}",
                serviceDetail.IdentityHost,
                serviceDetail.RequestIdentityPath,
                requestTokenResponse.Result));
        }

        throw new AuthorizedServiceException("Failed to obtain request token");
    }

    private bool CheckAuthorizationStatus(ServiceDetail serviceDetail) => serviceDetail.AuthenticationMethod switch
    {
        AuthenticationMethod.OAuth1 => StoredOAuth1TokenExists(serviceDetail),
        AuthenticationMethod.OAuth2AuthorizationCode => StoredOAuth2TokenExists(serviceDetail),
        AuthenticationMethod.OAuth2ClientCredentials => StoredOAuth2TokenExists(serviceDetail),
        AuthenticationMethod.ApiKey => !string.IsNullOrEmpty(serviceDetail.ApiKey)
                                       || _keyStorage.GetKey(serviceDetail.Alias) is not null,
        _ => false
    };

    private bool StoredOAuth2TokenExists(ServiceDetail serviceDetail) => _oauth2TokenStorage.GetToken(serviceDetail.Alias) != null;

    private bool StoredOAuth1TokenExists(ServiceDetail serviceDetail) => _oauth1TokenStorage.GetToken(serviceDetail.Alias) != null;
}
