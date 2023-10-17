using Microsoft.AspNetCore.Mvc;
using Umbraco.AuthorizedServices.Extensions;
using Umbraco.AuthorizedServices.Services;
using Umbraco.AuthorizedServices.TestSite.Models.ServiceResponses;
using Umbraco.Cms.Core;

namespace Umbraco.AuthorizedServices.TestSite.Controllers;

public class TestAuthorizedServicesController : AuthorizedServicesApiControllerBase
{
    public TestAuthorizedServicesController(IAuthorizedServiceCaller authorizedServiceCaller)
        : base(authorizedServiceCaller)
    {
    }

    public async Task<IActionResult> GetUmbracoContributorsFromGitHub()
    {
        Attempt<List<GitHubContributorResponse>?> responseAttempt = await AuthorizedServiceCaller.GetRequestAsync<List<GitHubContributorResponse>>(
            "github",
            "/repos/Umbraco/Umbraco-CMS/contributors");
        if (!responseAttempt.Success || responseAttempt.Result is null)
        {
            return HandleFailedRequest(responseAttempt.Exception, "Could not retrieve contributors.");
        }

        List<GitHubContributorResponse> response = responseAttempt.Result;
        return Content(string.Join(", ", response.Select(x => x.Login)));
    }

    public async Task<IActionResult> GetContactsFromHubspot()
    {
        Attempt<HubspotContactResponse?> responseAttempt = await AuthorizedServiceCaller.GetRequestAsync<HubspotContactResponse>(
            "hubspot",
            "/crm/v3/objects/contacts?limit=10&archived=false");
        if (!responseAttempt.Success || responseAttempt.Result is null)
        {
            return HandleFailedRequest(responseAttempt.Exception, "Could not retrieve contacts.");
        }

        HubspotContactResponse response = responseAttempt.Result;
        return Content(
            string.Join(
                ", ",
                response.Results
                    .Select(x => x.Properties.FirstName + " " + x.Properties.LastName)));
    }

    public async Task<IActionResult> GetMeetupSelfUserInfo()
    {
        // This makes a GraphQL query
        Attempt<string?> responseAttempt = await AuthorizedServiceCaller.SendRequestRawAsync(
            "meetup",
            "/gql",
            HttpMethod.Post,
            new MeetupRequest
            {
                Query = "query { self { id name bio city } }"
            });

        if (!responseAttempt.Success || responseAttempt.Result is null)
        {
            return HandleFailedRequest(responseAttempt.Exception, "Could not retrieve user info.");
        }

        var response = responseAttempt.Result;
        return Content(response);
    }

    public async Task<IActionResult> GetFormsFromDynamics()
    {
        Attempt<DynamicsFormResponse?> responseAttempt = await AuthorizedServiceCaller.GetRequestAsync<DynamicsFormResponse>(
            "dynamics",
            "/msdyncrm_marketingforms");

        if (!responseAttempt.Success || responseAttempt.Result is null)
        {
            return HandleFailedRequest(responseAttempt.Exception, "Could not retrieve forms.");
        }

        DynamicsFormResponse response = responseAttempt.Result;
        return Content(string.Join(", ", response.Results.Select(x => x.Name)));
    }

    public async Task<IActionResult> GetSearchResultsFromGoogle()
    {
        Attempt<string?> responseAttempt = await AuthorizedServiceCaller.SendRequestRawAsync(
            "google",
            "/v1/urlInspection/index:inspect",
            HttpMethod.Post,
            new GoogleSearchResponse
            {
                InspectionUrl = "https://umbraco.dk",
                SiteUrl = "https://umbraco.dk/products"
            });

        if (!responseAttempt.Success || responseAttempt.Result is null)
        {
            return HandleFailedRequest(responseAttempt.Exception, "Could not retrieve search results.");
        }

        var response = responseAttempt.Result;
        return Content(response);
    }

    public async Task<IActionResult> GetFoldersFromDropbox()
    {
        Attempt<string?> responseAttempt = await AuthorizedServiceCaller.SendRequestRawAsync(
            "dropbox",
            "/2/files/list_folder",
            HttpMethod.Post,
            new DropboxFolderResponse
            {
                IncludeDeleted = false,
                IncludeMediaInfo = true,
                Path = string.Empty
            });
        if (!responseAttempt.Success || responseAttempt.Result is null)
        {
            return HandleFailedRequest(responseAttempt.Exception, "Could not retrieve folders.");
        }

        var response = responseAttempt.Result;
        return Content(response);
    }

    public async Task<IActionResult> GetAssetsFromAssetBank(string assetIds)
    {
        Attempt<AssetBankSearchResponse?> responseAttempt = await AuthorizedServiceCaller.GetRequestAsync<AssetBankSearchResponse>(
            "assetBank",
            "/assetbank-rya-assets-test/rest/asset-search?assetIds=" + assetIds);

        if (!responseAttempt.Success || responseAttempt.Result is null)
        {
            return HandleFailedRequest(responseAttempt.Exception, "Could not retrieve assets.");
        }

        AssetBankSearchResponse response = responseAttempt.Result;
        return Content(string.Join(", ", response.Select(x => x.ToString())));
    }

    public async Task<IActionResult> GetVideoDetailsFromYouTube()
    {
        Attempt<string?> responseAttempt = await AuthorizedServiceCaller.SendRequestRawAsync(
            "youtube",
            "/v3/videos?id=[video_id]&part=snippet,contentDetails,statistics,status",
            HttpMethod.Get);

        if (!responseAttempt.Success || responseAttempt.Result is null)
        {
            return HandleFailedRequest(responseAttempt.Exception, "Could not retrieve video details.");
        }

        var response = responseAttempt.Result;
        return Content(response);
    }

    public IActionResult GetApiKey(string serviceAlias)
    {
        Attempt<string?> apiKeyAttempt = AuthorizedServiceCaller.GetApiKey(serviceAlias);
        return Content(apiKeyAttempt.Result ?? string.Empty);
    }

    public IActionResult GetOAuthToken(string serviceAlias)
    {
        Attempt<string?> responseAttempt = AuthorizedServiceCaller.GetOAuth2AccessToken(serviceAlias);
        if (!responseAttempt.Success || responseAttempt.Result is null)
        {
            return HandleFailedRequest(responseAttempt.Exception, "Could not retrieve access token.");
        }

        var response = responseAttempt.Result;
        return Content(response);
    }

    public IActionResult GetOAuth1Token(string serviceAlias)
    {
        Attempt<string?> responseAttempt = AuthorizedServiceCaller.GetOAuth1Token(serviceAlias);
        if (!responseAttempt.Success || responseAttempt.Result is null)
        {
            return Problem("Could not retrieve the OAuth token.");
        }

        var response = responseAttempt.Result;
        return Content(response);
    }
}
