using Microsoft.AspNetCore.Mvc;
using Umbraco.AuthorizedServices.Services;
using Umbraco.AuthorizedServices.TestSite.Models.ServiceResponses;
using Umbraco.Cms.Web.Common.Controllers;

namespace Umbraco.AuthorizedServices.TestSite.Controllers;

public class TestAuthorizedServicesController : UmbracoApiController
{
    private readonly IAuthorizedServiceCaller _authorizedServiceCaller;

    public TestAuthorizedServicesController(IAuthorizedServiceCaller authorizedServiceCaller) => _authorizedServiceCaller = authorizedServiceCaller;

    public async Task<IActionResult> GetUmbracoContributorsFromGitHub()
    {
        List<GitHubContributorResponse>? response = await _authorizedServiceCaller.SendRequestAsync<List<GitHubContributorResponse>>(
            "github",
            "/repos/Umbraco/Umbraco-CMS/contributors",
            HttpMethod.Get);
        if (response == null)
        {
            return Problem("Could not retrieve contributors.");
        }

        return Content(string.Join(", ", response.Select(x => x.Login)));
    }

    public async Task<IActionResult> GetContactsFromHubspot()
    {
        HubspotContactResponse? response = await _authorizedServiceCaller.SendRequestAsync<HubspotContactResponse>(
            "hubspot",
            "/crm/v3/objects/contacts?limit=10&archived=false",
            HttpMethod.Get);
        if (response == null)
        {
            return Problem("Could not retrieve contacts.");
        }

        return Content(string.Join(", ", response.Results.Select(x => x.Properties.FirstName + " " + x.Properties.LastName)));
    }

    public async Task<IActionResult> GetFormsFromDynamics()
    {
        DynamicsFormResponse? response = await _authorizedServiceCaller.SendRequestAsync<DynamicsFormResponse>(
            "dynamics",
            "/msdyncrm_marketingforms",
            HttpMethod.Get);

        if (response == null)
        {
            return Problem("Could not retrieve forms.");
        }

        return Content(string.Join(", ", response.Results.Select(x => x.Name)));
    }

    public async Task<IActionResult> GetSearchResultsFromGoogle()
    {
        var response = await _authorizedServiceCaller.SendRequestRawAsync(
            "google",
            "/v1/urlInspection/index:inspect",
            HttpMethod.Post,
            new GoogleSearchResponse
            {
                InspectionUrl = "https://umbraco.dk",
                SiteUrl = "https://umbraco.dk/products"
            });

        return Content(response);
    }
}

