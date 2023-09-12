using Microsoft.AspNetCore.Mvc;
using Umbraco.AuthorizedServices.Extensions;
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
        List<GitHubContributorResponse>? response = await _authorizedServiceCaller.GetRequestAsync<List<GitHubContributorResponse>>(
            "github",
            "/repos/Umbraco/Umbraco-CMS/contributors");
        if (response == null)
        {
            return Problem("Could not retrieve contributors.");
        }

        return Content(string.Join(", ", response.Select(x => x.Login)));
    }

    public async Task<IActionResult> GetContactsFromHubspot()
    {
        HubspotContactResponse? response = await _authorizedServiceCaller.GetRequestAsync<HubspotContactResponse>(
            "hubspot",
            "/crm/v3/objects/contacts?limit=10&archived=false");
        if (response == null)
        {
            return Problem("Could not retrieve contacts.");
        }

        return Content(
            string.Join(
                ", ",
                response.Results
                    .Select(x => x.Properties.FirstName + " " + x.Properties.LastName)));
    }

    public async Task<IActionResult> GetFormsFromDynamics()
    {
        DynamicsFormResponse? response = await _authorizedServiceCaller.GetRequestAsync<DynamicsFormResponse>(
            "dynamics",
            "/msdyncrm_marketingforms");

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

    public async Task<IActionResult> GetFoldersFromDropbox()
    {
        var response = await _authorizedServiceCaller.SendRequestRawAsync(
            "dropbox",
            "/2/files/list_folder",
            HttpMethod.Post,
            new DropboxFolderResponse
            {
                IncludeDeleted = false,
                IncludeMediaInfo = true,
                Path = ""
            });

        return Content(response);
    }

    public async Task<IActionResult> GetAssetsFromAssetBank(string assetIds)
    {
        AssetBankSearchResponse? response = await _authorizedServiceCaller.GetRequestAsync<AssetBankSearchResponse>(
            "assetBank",
            "/assetbank-rya-assets-test/rest/asset-search?assetIds=" + assetIds);

        if (response == null)
        {
            return Problem("Could not retrieve assets.");
        }

        return Content(string.Join(", ", response.Select(x => x.ToString())));
    }

    public async Task<IActionResult> GetVideoDetailsFromYouTube()
    {
        var response = await _authorizedServiceCaller.SendRequestRawAsync(
            "youtube",
            "/v3/videos?id=[video_id]&part=snippet,contentDetails,statistics,status",
            HttpMethod.Get);

        return Content(response);
    }

    public IActionResult GetApiKey(string serviceAlias)
    {
        var apiKey = _authorizedServiceCaller.GetApiKey(serviceAlias);
        return Content(apiKey ?? string.Empty);
    }
}

