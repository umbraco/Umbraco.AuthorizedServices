using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Umbraco.AuthorizedServices.Configuration;
using Umbraco.AuthorizedServices.Exceptions;
using Umbraco.AuthorizedServices.Helpers;
using Umbraco.AuthorizedServices.Models;
using Umbraco.AuthorizedServices.Models.Request;
using Umbraco.AuthorizedServices.Services;
using Umbraco.Cms.Core;
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
    public async Task<AuthorizedServiceDisplay?> GetByAlias(string alias)
    {
        ServiceDetail serviceDetail = _serviceDetailOptions.Get(alias);

        bool isAuthorized = await CheckAuthorizationStatus(serviceDetail);

        string? authorizationUrl = null;
        if (serviceDetail.AuthenticationMethod == AuthenticationMethod.OAuth2AuthorizationCode)
        {
            if (!isAuthorized)
            {
                AuthorizationPayload authorizationPayload = _authorizationPayloadBuilder.BuildPayload();

                var cacheKey = CacheHelper.GetPayloadKey(alias);
                _appCaches.RuntimeCache.Insert(cacheKey, () => authorizationPayload);

                authorizationUrl = _authorizationUrlBuilder
                    .BuildOAuth2AuthorizationUrl(serviceDetail, HttpContext, authorizationPayload.State, authorizationPayload.CodeChallenge);
            }
        }

        var settings = new Dictionary<string, string>
        {
            { nameof(ServiceDetail.Alias), serviceDetail.Alias },
            { nameof(ServiceDetail.DisplayName), serviceDetail.DisplayName },
            { nameof(ServiceDetail.AuthenticationMethod), serviceDetail.AuthenticationMethod.ToString() },
            { nameof(ServiceDetail.ClientCredentialsProvision), serviceDetail.ClientCredentialsProvision.ToString() },
            { nameof(ServiceDetail.ApiHost), serviceDetail.ApiHost },
            { nameof(ServiceDetail.IdentityHost), serviceDetail.IdentityHost },
            { nameof(ServiceDetail.TokenHost), serviceDetail.TokenHost },
            { nameof(ServiceDetail.CanManuallyProvideApiKey), serviceDetail.CanManuallyProvideApiKey? "Yes" : "No" },
            { nameof(ServiceDetail.CanManuallyProvideToken), serviceDetail.CanManuallyProvideToken ? "Yes" : "No" },
            { nameof(ServiceDetail.RequestAuthorizationPath), serviceDetail.RequestAuthorizationPath },
            { nameof(ServiceDetail.RequestIdentityPath), serviceDetail.RequestIdentityPath },
            { nameof(ServiceDetail.AuthorizationUrlRequiresRedirectUrl), serviceDetail.AuthorizationUrlRequiresRedirectUrl ? "Yes" : "No" },
            { nameof(ServiceDetail.RequestTokenPath), serviceDetail.RequestTokenPath },
            { nameof(ServiceDetail.RequestTokenMethod), serviceDetail.RequestTokenMethod.ToString() },
            { nameof(ServiceDetail.RequestTokenFormat), serviceDetail.RequestTokenFormat is not null ? serviceDetail.RequestTokenFormat.Value.ToString() : string.Empty },
            { nameof(ServiceDetail.JsonSerializer), serviceDetail.JsonSerializer.ToString() },
            { nameof(ServiceDetail.AuthorizationRequestRequiresAuthorizationHeaderWithBasicToken), serviceDetail.AuthorizationRequestRequiresAuthorizationHeaderWithBasicToken ? "Yes" : "No" },
            { nameof(ServiceDetail.ApiKey), new string('*', serviceDetail.ApiKey.Length) },
            { nameof(ServiceDetail.ApiKeyProvision), serviceDetail.ApiKeyProvision is not null ? serviceDetail.ApiKeyProvision.ToString() : string.Empty },
            { nameof(ServiceDetail.ClientId), serviceDetail.ClientId },
            { nameof(ServiceDetail.ClientSecret), new string('*', serviceDetail.ClientSecret.Length) },
            { nameof(ServiceDetail.Scopes), serviceDetail.Scopes },
            { nameof(ServiceDetail.IncludeScopesInAuthorizationRequest), serviceDetail.IncludeScopesInAuthorizationRequest ? "Yes" : "No" },
            { nameof(ServiceDetail.UseProofKeyForCodeExchange), serviceDetail.UseProofKeyForCodeExchange ? "Yes" : "No" },
            { nameof(ServiceDetail.CanExchangeToken), serviceDetail.CanExchangeToken ? "Yes" : "No" },
            { nameof(ServiceDetail.ExchangeTokenProvision) + ":" + nameof(ServiceDetail.ExchangeTokenProvision.TokenHost), serviceDetail.ExchangeTokenProvision?.TokenHost ?? string.Empty },
            { nameof(ServiceDetail.ExchangeTokenProvision) + ":" + nameof(ServiceDetail.ExchangeTokenProvision.RequestTokenPath), serviceDetail.ExchangeTokenProvision?.RequestTokenPath ?? string.Empty },
            { nameof(ServiceDetail.ExchangeTokenProvision) + ":" + nameof(ServiceDetail.ExchangeTokenProvision.TokenGrantType), serviceDetail.ExchangeTokenProvision?.TokenGrantType ?? string.Empty },
            { nameof(ServiceDetail.ExchangeTokenProvision) + ":" + nameof(ServiceDetail.ExchangeTokenProvision.RequestRefreshTokenPath), serviceDetail.ExchangeTokenProvision?.RequestRefreshTokenPath ?? string.Empty },
            { nameof(ServiceDetail.ExchangeTokenProvision) + ":" + nameof(ServiceDetail.ExchangeTokenProvision.RefreshTokenGrantType), serviceDetail.ExchangeTokenProvision?.RefreshTokenGrantType ?? string.Empty },
            { nameof(ServiceDetail.ExchangeTokenProvision) + ":" + nameof(ServiceDetail.ExchangeTokenProvision.ExchangeTokenWhenExpiresWithin), serviceDetail.ExchangeTokenProvision?.ExchangeTokenWhenExpiresWithin.ToString() ?? string.Empty },
            { nameof(ServiceDetail.AccessTokenResponseKey), serviceDetail.AccessTokenResponseKey },
            { nameof(ServiceDetail.RefreshTokenResponseKey), serviceDetail.RefreshTokenResponseKey },
            { nameof(ServiceDetail.ExpiresInResponseKey), serviceDetail.ExpiresInResponseKey },
            {
                nameof(ServiceDetail.SampleRequest),
                !string.IsNullOrEmpty(serviceDetail.SampleRequest)
                    ? serviceDetail.SampleRequest
                    : string.Empty
            }
        };

        return new AuthorizedServiceDisplay
        {
            DisplayName = serviceDetail.DisplayName,
            IsAuthorized = isAuthorized,
            CanManuallyProvideToken = serviceDetail.CanManuallyProvideToken,
            CanManuallyProvideApiKey = serviceDetail.CanManuallyProvideApiKey,
            AuthorizationUrl = authorizationUrl,
            AuthenticationMethod = serviceDetail.AuthenticationMethod.ToString(),
            SampleRequest = serviceDetail.SampleRequest,
            Settings = settings.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value)
        };
    }

    /// <summary>
    /// Sends a sample request for an authorized service.
    /// </summary>
    /// <param name="alias">The service alias.</param>
    /// <returns></returns>
    [HttpGet]
    public async Task<IActionResult> SendSampleRequest(string alias)
    {
        ServiceDetail serviceDetail = _serviceDetailOptions.Get(alias);

        Attempt<string?> responseAttempt = await _authorizedServiceCaller.SendRequestRawAsync(alias, serviceDetail.SampleRequest ?? string.Empty, HttpMethod.Get);
        if (responseAttempt.Success && responseAttempt.Result is not null)
        {
            return Ok(responseAttempt.Result);
        }

        if (responseAttempt.Exception is not null)
        {
            if (responseAttempt.Exception is AuthorizedServiceHttpException authorizedServiceHttpException)
            {
                return StatusCode((int)authorizedServiceHttpException.StatusCode, authorizedServiceHttpException.Reason + ": " + authorizedServiceHttpException.Content);
            }

            if (responseAttempt.Exception is AuthorizedServiceException authorizedServiceException)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, authorizedServiceException.Message);
            }
        }

        return StatusCode((int)HttpStatusCode.InternalServerError, "Could not complete the sample request due to an unexpected error");
    }

    /// <summary>
    /// Revokes access by removing the access token or API key for an authorized service.
    /// </summary>
    /// <param name="model">Request model identifying the service.</param>
    [HttpPost]
    public async Task<IActionResult> RevokeAccess(RevokeAccess model)
    {
        ServiceDetail serviceDetail = _serviceDetailOptions.Get(model.Alias);

        switch (serviceDetail.AuthenticationMethod)
        {
            case AuthenticationMethod.ApiKey:
                await _keyStorage.DeleteKeyAsync(model.Alias);
                ClearCachedApiKey(model.Alias);
                break;
            case AuthenticationMethod.OAuth1:
                await _oauth1TokenStorage.DeleteTokenAsync(model.Alias);
                ClearCachedToken(model.Alias);
                break;
            case AuthenticationMethod.OAuth2AuthorizationCode:
            case AuthenticationMethod.OAuth2ClientCredentials:
                await _oauth2TokenStorage.DeleteTokenAsync(model.Alias);
                ClearCachedToken(model.Alias);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(serviceDetail.AuthenticationMethod));
        }

        return Ok();
    }

    private void ClearCachedApiKey(string serviceAlias)
    {
        var cacheKey = CacheHelper.GetApiKeyCacheKey(serviceAlias);
        _appCaches.RuntimeCache.ClearByKey(cacheKey);
    }

    private void ClearCachedToken(string serviceAlias)
    {
        var cacheKey = CacheHelper.GetTokenCacheKey(serviceAlias);
        _appCaches.RuntimeCache.ClearByKey(cacheKey);
    }

    /// <summary>
    /// Adds a new access token for an OAuth2 authorized service.
    /// </summary>
    /// <param name="model">Request model identifying the service.</param>
    /// <returns></returns>
    [HttpPost]
    public async Task<IActionResult> SaveOAuth2Token(AddOAuth2Token model)
    {
        await _oauth2TokenStorage.SaveTokenAsync(model.Alias, new OAuth2Token(model.Token));
        return Ok();
    }

    /// <summary>
    /// Adds a new access token/token secret pair for an OAuth1 authorized service.
    /// </summary>
    /// <param name="model">Request model identifying the service.</param>
    /// <returns></returns>
    [HttpPost]
    public async Task<IActionResult> SaveOAuth1Token(AddOAuth1Token model)
    {
        await _oauth1TokenStorage.SaveTokenAsync(model.Alias, new OAuth1Token(model.Token, model.TokenSecret));
        return Ok();
    }

    /// <summary>
    /// Adds a new API key for an authorized service.
    /// </summary>
    /// <param name="model">Request model identifying the service.</param>
    /// <returns></returns>
    [HttpPost]
    public async Task<IActionResult> SaveApiKey(AddApiKey model)
    {
        await _keyStorage.SaveKeyAsync(model.Alias, model.ApiKey);
        return Ok();
    }

    /// <summary>
    /// Generates access token for an authorized service (using OAuth2).
    /// </summary>
    /// <param name="model">Request model identifying the service.</param>
    /// <returns></returns>
    [HttpPost]
    public async Task<IActionResult> GenerateOAuth2ClientCredentialsToken(GenerateToken model)
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
    public async Task<IActionResult> GenerateOAuth1RequestToken(GenerateToken model)
    {
        ServiceDetail serviceDetail = _serviceDetailOptions.Get(model.Alias);

        var url = _authorizationUrlBuilder.BuildOAuth1RequestTokenUrl(serviceDetail, HttpContext, HttpMethod.Post, OAuth1Helper.GetNonce(), OAuth1Helper.GetTimestamp());

        Models.AuthorizationResult requestTokenResponse = await _serviceAuthorizer.GenerateOAuth1RequestTokenAsync(model.Alias, url);

        if (requestTokenResponse.Success && requestTokenResponse.Result is not null && requestTokenResponse.Result.TryParseOAuth1Response(out var oauthToken, out var oauthTokenSecret))
        {
            _appCaches.RuntimeCache.InsertCacheItem(oauthToken, () => serviceDetail.Alias);

            _appCaches.RuntimeCache.InsertCacheItem(CacheHelper.GetTokenSecretCacheKey(serviceDetail.Alias), () => oauthTokenSecret);

            return Ok(string.Format(
                "{0}{1}?{2}",
                serviceDetail.IdentityHost,
                serviceDetail.RequestIdentityPath,
                requestTokenResponse.Result));
        }

        throw new AuthorizedServiceException("Failed to obtain request token");
    }

    private async Task<bool> CheckAuthorizationStatus(ServiceDetail serviceDetail) => serviceDetail.AuthenticationMethod switch
    {
        AuthenticationMethod.OAuth1 => await StoredOAuth1TokenExists(serviceDetail),
        AuthenticationMethod.OAuth2AuthorizationCode => await StoredOAuth2TokenExists(serviceDetail),
        AuthenticationMethod.OAuth2ClientCredentials => await StoredOAuth2TokenExists(serviceDetail),
        AuthenticationMethod.ApiKey => !string.IsNullOrEmpty(serviceDetail.ApiKey)
                                       || await _keyStorage.GetKeyAsync(serviceDetail.Alias) is not null,
        _ => false
    };

    private async Task<bool> StoredOAuth2TokenExists(ServiceDetail serviceDetail) => await _oauth2TokenStorage.GetTokenAsync(serviceDetail.Alias) != null;

    private async Task<bool> StoredOAuth1TokenExists(ServiceDetail serviceDetail) => await _oauth1TokenStorage.GetTokenAsync(serviceDetail.Alias) != null;
}
