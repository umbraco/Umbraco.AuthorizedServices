using Microsoft.AspNetCore.Mvc;
using Umbraco.AuthorizedServices.Extensions;
using Umbraco.AuthorizedServices.Models;
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
        Attempt<AuthorizedServiceResponse<List<GitHubContributorResponse>>> responseAttempt = await AuthorizedServiceCaller.GetRequestAsync<List<GitHubContributorResponse>>(
            "github",
            "/repos/Umbraco/Umbraco-CMS/contributors");
        if (!responseAttempt.Success || responseAttempt.Result is null)
        {
            return HandleFailedRequest(responseAttempt.Exception, "Could not retrieve contributors.");
        }

        List<GitHubContributorResponse> response = responseAttempt.Result.Data!;
        return Content(string.Join(", ", response.Select(x => x.Login)));
    }

    public async Task<IActionResult> GetContactsFromHubspot()
    {
        Attempt<AuthorizedServiceResponse<HubspotContactResponse>> responseAttempt = await AuthorizedServiceCaller.GetRequestAsync<HubspotContactResponse>(
            "hubspot",
            "/crm/v3/objects/contacts?limit=10&archived=false");
        if (!responseAttempt.Success || responseAttempt.Result is null)
        {
            return HandleFailedRequest(responseAttempt.Exception, "Could not retrieve contacts.");
        }

        HubspotContactResponse response = responseAttempt.Result.Data!;
        return Content(
            string.Join(
                ", ",
                response.Results
                    .Select(x => x.Properties.FirstName + " " + x.Properties.LastName)));
    }

    public async Task<IActionResult> GetMeetupSelfUserInfo()
    {
        // This makes a GraphQL query
        Attempt<AuthorizedServiceResponse<string>> responseAttempt = await AuthorizedServiceCaller.SendRequestRawAsync(
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

        AuthorizedServiceResponse<string> response = responseAttempt.Result;
        return Content(response.Data ?? string.Empty);
    }

    public async Task<IActionResult> GetFormsFromDynamics()
    {
        Attempt<AuthorizedServiceResponse<DynamicsFormResponse>> responseAttempt = await AuthorizedServiceCaller.GetRequestAsync<DynamicsFormResponse>(
            "dynamics",
            "/msdyncrm_marketingforms");

        if (!responseAttempt.Success || responseAttempt.Result is null)
        {
            return HandleFailedRequest(responseAttempt.Exception, "Could not retrieve forms.");
        }

        DynamicsFormResponse response = responseAttempt.Result.Data!;
        return Content(string.Join(", ", response.Results.Select(x => x.Name)));
    }

    public async Task<IActionResult> GetSearchResultsFromGoogle()
    {
        Attempt<AuthorizedServiceResponse<string>> responseAttempt = await AuthorizedServiceCaller.SendRequestRawAsync(
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
        return Content(response.Data ?? string.Empty);
    }

    public async Task<IActionResult> GetFoldersFromDropbox()
    {
        Attempt<AuthorizedServiceResponse<string>> responseAttempt = await AuthorizedServiceCaller.SendRequestRawAsync(
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

        var response = responseAttempt.Result.Data;
        return Content(response ?? string.Empty);
    }

    public async Task<IActionResult> GetAssetsFromAssetBank(string assetIds)
    {
        Attempt<AuthorizedServiceResponse<AssetBankSearchResponse>> responseAttempt = await AuthorizedServiceCaller.GetRequestAsync<AssetBankSearchResponse>(
            "assetBank",
            "/assetbank-rya-assets-test/rest/asset-search?assetIds=" + assetIds);

        if (!responseAttempt.Success || responseAttempt.Result is null)
        {
            return HandleFailedRequest(responseAttempt.Exception, "Could not retrieve assets.");
        }

        AssetBankSearchResponse response = responseAttempt.Result.Data!;
        return Content(string.Join(", ", response.Select(x => x.ToString())));
    }

    public async Task<IActionResult> GetVideoDetailsFromYouTube(string videoId)
    {
        Attempt<AuthorizedServiceResponse<string>> responseAttempt = await AuthorizedServiceCaller.SendRequestRawAsync(
            "youtube",
            $"/v3/videos?id={videoId}&part=snippet,contentDetails,statistics,status",
            HttpMethod.Get);

        if (!responseAttempt.Success || responseAttempt.Result is null)
        {
            return HandleFailedRequest(responseAttempt.Exception, "Could not retrieve video details.");
        }

        var response = responseAttempt.Result.Data;
        return Content(response ?? string.Empty);
    }

    public async Task<IActionResult> GetInstagramProfile()
    {
        Attempt<AuthorizedServiceResponse<InstagramProfileResponse>> responseAttempt = await AuthorizedServiceCaller.GetRequestAsync<InstagramProfileResponse>(
            "instagram",
            $"/v3.0/me?fields=username");

        if (!responseAttempt.Success || responseAttempt.Result is null)
        {
            return HandleFailedRequest(responseAttempt.Exception, "Could not retrieve account details.");
        }

        return Content(responseAttempt.Result.Data!.Username);
    }

    public async Task<IActionResult> GetTwitterProfileUsingOAuth1()
    {
        Attempt<AuthorizedServiceResponse<string>> responseAttempt = await AuthorizedServiceCaller.SendRequestRawAsync(
            "twitter",
            "/1.1/account/settings.json",
            HttpMethod.Get);

        if (!responseAttempt.Success || responseAttempt.Result is null)
        {
            return HandleFailedRequest(responseAttempt.Exception, "Could not retrieve account details.");
        }

        var response = responseAttempt.Result.Data;
        return Content(response ?? string.Empty);
    }

    public async Task<IActionResult> GetTwitterProfileUsingOAuth2()
    {
        Attempt<AuthorizedServiceResponse<string>> responseAttempt = await AuthorizedServiceCaller.SendRequestRawAsync(
            "twitter_oauth2",
            "/2/users/me",
            HttpMethod.Get);

        if (!responseAttempt.Success || responseAttempt.Result is null)
        {
            return HandleFailedRequest(responseAttempt.Exception, "Could not retrieve account details.");
        }

        var response = responseAttempt.Result.Data;
        return Content(response ?? string.Empty);
    }

    public async Task<IActionResult> GetApiKey(string serviceAlias)
    {
        Attempt<string?> apiKeyAttempt = await AuthorizedServiceCaller.GetApiKey(serviceAlias);
        return Content(apiKeyAttempt.Result ?? string.Empty);
    }

    public async Task<IActionResult> GetOAuthToken(string serviceAlias)
    {
        Attempt<string?> responseAttempt = await AuthorizedServiceCaller.GetOAuth2AccessToken(serviceAlias);
        if (!responseAttempt.Success || responseAttempt.Result is null)
        {
            return HandleFailedRequest(responseAttempt.Exception, "Could not retrieve access token.");
        }

        var response = responseAttempt.Result;
        return Content(response);
    }

    public async Task<IActionResult> GetOAuth1Token(string serviceAlias)
    {
        Attempt<string?> responseAttempt = await AuthorizedServiceCaller.GetOAuth1Token(serviceAlias);
        if (!responseAttempt.Success || responseAttempt.Result is null)
        {
            return Problem("Could not retrieve the OAuth token.");
        }

        var response = responseAttempt.Result;
        return Content(response);
    }
}
