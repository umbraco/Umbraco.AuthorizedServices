using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Umbraco.AuthorizedServices.Configuration;
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
    private readonly AuthorizedServiceSettings _authorizedServiceSettings;
    private readonly ITokenStorage _tokenStorage;
    private readonly IAuthorizationUrlBuilder _authorizationUrlBuilder;
    private readonly IAuthorizedServiceCaller _authorizedServiceCaller;
    private readonly IAuthorizationPayloadCache _authorizedServiceAuthorizationPayloadCache;
    private readonly IAuthorizationPayloadBuilder _authorizedServiceAuthorizationPayloadBuilder;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthorizedServiceController"/> class.
    /// </summary>
    public AuthorizedServiceController(
        IOptionsMonitor<AuthorizedServiceSettings> authorizedServiceSettings,
        ITokenStorage tokenStorage,
        IAuthorizationUrlBuilder authorizationUrlBuilder,
        IAuthorizedServiceCaller authorizedServiceCaller,
        IAuthorizationPayloadCache authorizedServiceAuthorizationPayloadCache,
        IAuthorizationPayloadBuilder authorizedServiceAuthorizationPayloadBuilder)
    {
        _authorizedServiceSettings = authorizedServiceSettings.CurrentValue;
        _tokenStorage = tokenStorage;
        _authorizationUrlBuilder = authorizationUrlBuilder;
        _authorizedServiceCaller = authorizedServiceCaller;
        _authorizedServiceAuthorizationPayloadCache = authorizedServiceAuthorizationPayloadCache;
        _authorizedServiceAuthorizationPayloadBuilder = authorizedServiceAuthorizationPayloadBuilder;
    }

    /// <summary>
    /// Retrieves the details for a service by alias.
    /// </summary>
    /// <param name="alias">The service alias.</param>
    [HttpGet]
    public AuthorizedServiceDisplay? GetByAlias(string alias)
    {
        ServiceDetail? serviceDetail = _authorizedServiceSettings.Services.SingleOrDefault(x => x.Alias == alias);
        if (serviceDetail == null)
        {
            return null;
        }

        bool tokenExists = _tokenStorage.GetToken(alias) != null;

        string? authorizationUrl = null;
        if (!tokenExists)
        {
            AuthorizationPayload authorizationPayload = _authorizedServiceAuthorizationPayloadBuilder.BuildPayload();

            _authorizedServiceAuthorizationPayloadCache.Add(alias, authorizationPayload);

            authorizationUrl = _authorizationUrlBuilder
                .BuildUrl(serviceDetail, HttpContext, authorizationPayload.State, authorizationPayload.CodeChallenge);
        }

        return new AuthorizedServiceDisplay
        {
            DisplayName = serviceDetail.DisplayName,
            IsAuthorized = tokenExists,
            AuthorizationUrl = authorizationUrl,
            SampleRequest = serviceDetail.SampleRequest,
            Settings = new Dictionary<string, string>
            {
                { nameof(ServiceDetail.Alias), serviceDetail.Alias },
                { nameof(ServiceDetail.DisplayName), serviceDetail.DisplayName },
                { nameof(ServiceDetail.ApiHost), serviceDetail.ApiHost },
                { nameof(ServiceDetail.IdentityHost), serviceDetail.IdentityHost },
                { nameof(ServiceDetail.TokenHost), serviceDetail.TokenHost },
                { nameof(ServiceDetail.RequestIdentityPath), serviceDetail.RequestIdentityPath },
                { nameof(ServiceDetail.RequestTokenPath), serviceDetail.RequestTokenPath },
                { nameof(ServiceDetail.RequestTokenFormat), serviceDetail.RequestTokenFormat.ToString() },
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
    /// Revokes access by removing the access token for an authorized service.
    /// </summary>
    /// <param name="model">Request model identifying the service.</param>
    [HttpPost]
    public IActionResult RevokeAccess(RevokeAccess model)
    {
        _tokenStorage.DeleteToken(model.Alias);
        return Ok();
    }
}
