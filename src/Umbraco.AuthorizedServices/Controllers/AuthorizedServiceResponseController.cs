using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Umbraco.AuthorizedServices.Configuration;
using Umbraco.AuthorizedServices.Exceptions;
using Umbraco.AuthorizedServices.Extensions;
using Umbraco.AuthorizedServices.Models;
using Umbraco.AuthorizedServices.Services;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Web.Common.Controllers;

namespace Umbraco.AuthorizedServices.Controllers;

/// <summary>
/// Controller that handles the returning messages for the authorization flow with an external service.
/// </summary>
public class AuthorizedServiceResponseController : UmbracoApiController
{
    private readonly IAuthorizedServiceAuthorizer _serviceAuthorizer;
    private readonly IOptionsMonitor<ServiceDetail> _serviceDetailOptions;
    private readonly AppCaches _appCaches;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthorizedServiceResponseController"/> class.
    /// </summary>
    public AuthorizedServiceResponseController(
        IAuthorizedServiceAuthorizer serviceAuthorizer,
        IOptionsMonitor<ServiceDetail> serviceDetailOptions,
        AppCaches appCaches)
    {
        _serviceAuthorizer = serviceAuthorizer;
        _serviceDetailOptions = serviceDetailOptions;
        _appCaches = appCaches;
    }

    /// <summary>
    /// Handles the returning messages for the authorization flow with an external service.
    /// </summary>
    /// <param name="code">The authorization code.</param>
    /// <param name="state">The state.</param>
    public async Task<IActionResult> HandleOAuth2IdentityResponse(string code, string state)
    {
        var stateParts = state.Split(Constants.Separator);
        if (stateParts.Length != 2)
        {
            throw new AuthorizedServiceException("The state provided in the identity response could not be parsed.");
        }

        var cacheKey = string.Format(Constants.Cache.AuthorizationPayloadKeyFormat, stateParts[0]);
        if (_appCaches.RuntimeCache.Get(cacheKey) is not AuthorizationPayload cachedAuthorizationPayload || stateParts[1] != cachedAuthorizationPayload.State)
        {
            throw new AuthorizedServiceException("The state provided in the identity response did not match the expected value.");
        }

        var serviceAlias = stateParts[0];

        var redirectUri = HttpContext.GetOAuth2AuthorizedServiceRedirectUri();
        var codeVerifier = cachedAuthorizationPayload.CodeVerifier;
        _appCaches.RuntimeCache.ClearByKey(cacheKey);
        AuthorizationResult result = await _serviceAuthorizer.AuthorizeOAuth2AuthorizationCodeServiceAsync(serviceAlias, code, redirectUri, codeVerifier);

        // Handle exchange of short for long-lived token if configured.
        ServiceDetail serviceDetail = _serviceDetailOptions.Get(serviceAlias);
        if (serviceDetail.CanExchangeToken)
        {
            return await HandleTokenExchange(serviceDetail);
        }

        if (result.Success)
        {
            return Redirect($"/umbraco#/settings/AuthorizedServices/edit/{serviceAlias}");
        }

        throw new AuthorizedServiceException("Failed to obtain access token");
    }

    /// <summary>
    /// Handles the returning message for OAuth1 authorization flow with an external service.
    /// </summary>
    /// <param name="oauth_token">The oauth_token.</param>
    /// <param name="oauth_verifier">The oauth_verifier.</param>
    /// <returns></returns>
    /// <exception cref="AuthorizedServiceException"></exception>
    public async Task<IActionResult> HandleOAuth1IdentityResponse(string oauth_token, string oauth_verifier)
    {
        var serviceAlias = _appCaches.RuntimeCache.Get(oauth_token) as string
            ?? throw new AuthorizedServiceException("No cached service with the specified token was found.");

        AuthorizationResult result = await _serviceAuthorizer.AuthorizeOAuth1ServiceAsync(serviceAlias, oauth_token, oauth_verifier);
        if (result.Success)
        {
            return Redirect($"/umbraco#/settings/AuthorizedServices/edit/{serviceAlias}");
        }

        throw new AuthorizedServiceException("Failed to obtain access token");
    }

    /// <summary>
    /// Handles the token exchange flow.
    /// </summary>
    /// <param name="serviceDetail">The service detail.</param>
    private async Task<IActionResult> HandleTokenExchange(ServiceDetail serviceDetail)
    {
        if (serviceDetail.ExchangeTokenProvision is null)
        {
            throw new AuthorizedServiceException("Failed to retrieve exchange token provisioning.");
        }

        AuthorizationResult exchangeResult = await _serviceAuthorizer.ExchangeOAuth2AccessTokenAsync(serviceDetail.Alias);

        if (exchangeResult.Success)
        {
            return Redirect($"/umbraco#/settings/AuthorizedServices/edit/{serviceDetail.Alias}");
        }

        throw new AuthorizedServiceException("Failed to exchange the access token.");
    }
}
