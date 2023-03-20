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

[PluginController(Constants.PluginName)]
[Authorize(Policy = Cms.Web.Common.Authorization.AuthorizationPolicies.SectionAccessSettings)]
public class AuthorizedServiceController : BackOfficeNotificationsController
{
    private readonly AuthorizedServiceSettings _authorizedServiceSettings;
    private readonly ITokenStorage _tokenStorage;
    private readonly IAuthorizationUrlBuilder _authorizationUrlBuilder;
    private readonly IAuthorizedServiceCaller _authorizedServiceCaller;

    public AuthorizedServiceController(
        IOptionsMonitor<AuthorizedServiceSettings> authorizedServiceSettings,
        ITokenStorage tokenStorage,
        IAuthorizationUrlBuilder authorizationUrlBuilder,
        IAuthorizedServiceCaller authorizedServiceCaller)
    {
        _authorizedServiceSettings = authorizedServiceSettings.CurrentValue;
        _tokenStorage = tokenStorage;
        _authorizationUrlBuilder = authorizationUrlBuilder;
        _authorizedServiceCaller = authorizedServiceCaller;
    }

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
            authorizationUrl = _authorizationUrlBuilder.BuildUrl(serviceDetail, HttpContext);
        }

        return new AuthorizedServiceDisplay
        {
            DisplayName = serviceDetail.DisplayName,
            IsAuthorized = tokenExists,
            AuthorizationUrl = authorizationUrl,
            SampleRequest = serviceDetail.SampleRequest
        };
    }

    [HttpGet]
    public async Task<IActionResult> SendSampleRequest(string alias, string path)
    {
        string response = await _authorizedServiceCaller.SendRequestRawAsync(alias, path, HttpMethod.Get);
        return Ok(response);
    }

    [HttpPost]
    public IActionResult RevokeAccess(RevokeAccess model)
    {
        _tokenStorage.DeleteToken(model.Alias);
        return Ok();
    }
}
