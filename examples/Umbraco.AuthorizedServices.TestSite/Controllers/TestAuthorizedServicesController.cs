using Microsoft.AspNetCore.Mvc;
using Umbraco.AuthorizedServices.Services;
using Umbraco.AuthorizedServices.TestSite.Models;
using Umbraco.Cms.Web.Common.Controllers;

namespace Umbraco.AuthorizedServices.TestSite.Controllers;

public class TestAuthorizedServicesController : UmbracoApiController
{
    private readonly IAuthorizedServiceCaller _authorizedServiceCaller;

    public TestAuthorizedServicesController(IAuthorizedServiceCaller authorizedServiceCaller) => _authorizedServiceCaller = authorizedServiceCaller;

    public async Task<IActionResult> GetUmbracoContributorsFromGitHub()
    {
        List<GitHubContributorResponse> response = await _authorizedServiceCaller.SendRequestAsync<List<GitHubContributorResponse>>(
            "github",
            "/repos/Umbraco/Umbraco-CMS/contributors",
            HttpMethod.Get);
        return Content(string.Join(", ", response.Select(x => x.Login)));
    }

    public async Task<IActionResult> GetContactsFromHubspot()
    {
        HubspotContactResponse response = await _authorizedServiceCaller.SendRequestAsync<HubspotContactResponse>(
            "hubspot",
            "/crm/v3/objects/contacts?limit=10&archived=false",
            HttpMethod.Get);
        return Content(string.Join(", ", response.Results.Select(x => x.Properties.FirstName + " " + x.Properties.LastName)));
    }
}

