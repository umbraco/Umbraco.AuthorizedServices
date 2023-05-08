using Microsoft.AspNetCore.Mvc;
using Umbraco.AuthorizedServices.Extensions;
using Umbraco.AuthorizedServices.Services;
using Umbraco.AuthorizedServices.TestSite.Models.Dtos;
using Umbraco.AuthorizedServices.TestSite.Models.ServiceResponses;
using Umbraco.Cms.Web.Common.Controllers;

namespace Umbraco.AuthorizedServices.TestSite.Controllers;

[Route("umbraco/authorizedservice/hubspot/v1/contacts")]
public class HubspotContactsController : UmbracoApiController
{
    private const string ServiceAlias = "hubspot";
    private const string BasePath = "/crm/v3/objects/contacts/";

    private readonly IAuthorizedServiceCaller _authorizedServiceCaller;

    public HubspotContactsController(IAuthorizedServiceCaller authorizedServiceCaller) => _authorizedServiceCaller = authorizedServiceCaller;

    [HttpGet]
    public async Task<IActionResult> Get()
    {
       HubspotContactResponse? response = await _authorizedServiceCaller.GetRequestAsync<HubspotContactResponse>(
            ServiceAlias,
            BasePath);
        if (response == null)
        {
            return Problem("Could not retrieve contacts.");
        }

        return Ok(
            response.Results
                .Select(MapToDto)
                .ToList());
    }

    [HttpGet]
    [Route("{id}")]
    public async Task<IActionResult> Get(string id)
    {
        HubspotContactResponse.Result? response = await _authorizedServiceCaller.GetRequestAsync<HubspotContactResponse.Result>(
            ServiceAlias,
            $"{BasePath}{id}");
        if (response == null)
        {
            return NotFound();
        }

        return Ok(MapToDto(response));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ContactDto contact)
    {
        HubspotContactResponse.Result? response = await _authorizedServiceCaller.PostRequestAsync<HubspotContactResponse.Result, HubspotContactResponse.Result>(
            ServiceAlias,
            BasePath,
            MapToRequest(contact));
        if (response == null)
        {
            return Problem("Could not create contact.");
        }

        return CreatedAtAction(nameof(Get), new { id = response.Id }, MapToDto(response));
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] ContactDto contact)
    {
        HubspotContactResponse.Result? response = await _authorizedServiceCaller.PatchRequestAsync<HubspotContactResponse.Result, HubspotContactResponse.Result>(
            ServiceAlias,
            $"{BasePath}{contact.Id}",
            MapToRequest(contact));
        if (response == null)
        {
            return Problem("Could not update contact.");
        }

        return Ok(MapToDto(response));
    }

    [HttpDelete]
    [Route("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        await _authorizedServiceCaller.DeleteRequestAsync(
            ServiceAlias,
            $"{BasePath}{id}");

        return NoContent();
    }

    private ContactDto MapToDto(HubspotContactResponse.Result result) =>
        new ContactDto
        {
            Id = result.Id,
            FirstName = result.Properties.FirstName,
            LastName = result.Properties.LastName,
            Email = result.Properties.Email
        };

    private HubspotContactResponse.Result MapToRequest(ContactDto dto) =>
        new HubspotContactResponse.Result
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
