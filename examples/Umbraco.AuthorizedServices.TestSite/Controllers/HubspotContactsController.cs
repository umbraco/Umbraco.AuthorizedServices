using Microsoft.AspNetCore.Mvc;
using Umbraco.AuthorizedServices.Extensions;
using Umbraco.AuthorizedServices.Services;
using Umbraco.AuthorizedServices.TestSite.Models.Dtos;
using Umbraco.AuthorizedServices.TestSite.Models.ServiceResponses;
using Umbraco.Cms.Core;

namespace Umbraco.AuthorizedServices.TestSite.Controllers;

[Route("umbraco/authorizedservice/hubspot/v1/contacts")]
public class HubspotContactsController : AuthorizedServicesApiControllerBase
{
    private const string ServiceAlias = "hubspot";
    private const string BasePath = "/crm/v3/objects/contacts/";

    public HubspotContactsController(IAuthorizedServiceCaller authorizedServiceCaller)
        : base(authorizedServiceCaller)
    {
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
       Attempt<HubspotContactResponse?> responseAttempt = await AuthorizedServiceCaller.GetRequestAsync<HubspotContactResponse>(
            ServiceAlias,
            BasePath);
        if (!responseAttempt.Success || responseAttempt.Result is null)
        {
            return HandleFailedRequest(responseAttempt.Exception, "Could not retrieve contacts.");
        }

        HubspotContactResponse response = responseAttempt.Result;
        return Ok(
            response.Results
                .Select(MapToDto)
                .ToList());
    }

    [HttpGet]
    [Route("{id}")]
    public async Task<IActionResult> Get(string id)
    {
        Attempt<HubspotContactResponse.Result?> responseAttempt = await AuthorizedServiceCaller.GetRequestAsync<HubspotContactResponse.Result>(
            ServiceAlias,
            $"{BasePath}{id}");

        if (!responseAttempt.Success || responseAttempt.Result is null)
        {
            return HandleFailedRequest(responseAttempt.Exception, "Could not retrieve contact.");
        }

        HubspotContactResponse.Result response = responseAttempt.Result;
        return Ok(MapToDto(response));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ContactDto contact)
    {
        Attempt<HubspotContactResponse.Result?> responseAttempt = await AuthorizedServiceCaller.PostRequestAsync<HubspotContactResponse.Result, HubspotContactResponse.Result>(
            ServiceAlias,
            BasePath,
            MapToRequest(contact));
        if (!responseAttempt.Success || responseAttempt.Result is null)
        {
            return HandleFailedRequest(responseAttempt.Exception, "Could not create contact.");
        }

        HubspotContactResponse.Result response = responseAttempt.Result;
        return CreatedAtAction(nameof(Get), new { id = response.Id }, MapToDto(response));
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] ContactDto contact)
    {
        Attempt<HubspotContactResponse.Result?> responseAttempt = await AuthorizedServiceCaller.PatchRequestAsync<HubspotContactResponse.Result, HubspotContactResponse.Result>(
            ServiceAlias,
            $"{BasePath}{contact.Id}",
            MapToRequest(contact));
        if (!responseAttempt.Success || responseAttempt.Result is null)
        {
            return HandleFailedRequest(responseAttempt.Exception, "Could not update contact.");
        }

        HubspotContactResponse.Result response = responseAttempt.Result;
        return Ok(MapToDto(response));
    }

    [HttpDelete]
    [Route("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        await AuthorizedServiceCaller.DeleteRequestAsync(
            ServiceAlias,
            $"{BasePath}{id}");

        return NoContent();
    }

    private ContactDto MapToDto(HubspotContactResponse.Result result) =>
        new()
        {
            Id = result.Id,
            FirstName = result.Properties.FirstName,
            LastName = result.Properties.LastName,
            Email = result.Properties.Email
        };

    private HubspotContactResponse.Result MapToRequest(ContactDto dto) =>
        new()
        {
            Id = dto.Id,
            Properties = new HubspotContactResponse.ResultProperties
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email
            }
        };
}
