using System.Net.Http.Headers;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.AuthorizedServices.Services;
using Umbraco.Cms.Web.Common.Controllers;
using Umbraco.AuthorizedServices.TestSite.Models;
using static Umbraco.AuthorizedServices.TestSite.Models.HubspotContactResponse;
using System.Text.Json;

namespace Umbraco.AuthorizedServices.TestSite.Controllers;

[Route("umbraco/authorizedservice/hubspot/v1/contacts")]
public class HubspotContactsController : UmbracoApiController
{
    private readonly IAuthorizedServiceCaller _authorizedServiceCaller;

    public HubspotContactsController(IAuthorizedServiceCaller authorizedServiceCaller) => _authorizedServiceCaller = authorizedServiceCaller;

    [HttpGet]
    public async Task<IActionResult> Get()
    {
       HubspotContactResponse response = await _authorizedServiceCaller.SendRequestAsync<HubspotContactResponse>(
            "hubspot",
            "/crm/v3/objects/contacts",
            HttpMethod.Get);

        return Ok(response.Results);
    }

    [HttpGet]
    [Route("{id}")]
    public async Task<IActionResult> Get(string id)
    {
        Result response = await _authorizedServiceCaller.SendRequestAsync<Result>(
            "hubspot",
            $"/crm/v3/objects/contacts/{id}",
            HttpMethod.Get);

        return Ok(response);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Result contact)
    {
        var response = await _authorizedServiceCaller.SendRequestRawAsync(
            "hubspot",
            "/crm/v3/objects/contacts",
            HttpMethod.Post,
            contact);

        Result? result = JsonSerializer.Deserialize<Result>(response);
        if(result != null)
        {
            contact.Id = result.Id;
        }

        return Created($"objects/contacts/{(result != null ? result.Id : string.Empty)}", contact);
    }

    [HttpPatch]
    [Route("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] Result contact)
    {
        await _authorizedServiceCaller.SendRequestRawAsync(
           "hubspot",
           $"/crm/v3/objects/contacts/{id}",
           HttpMethod.Patch,
           contact);

        return Ok();
    }

    [HttpDelete]
    [Route("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        await _authorizedServiceCaller.SendRequestRawAsync(
          "hubspot",
          $"/crm/v3/objects/contacts/{id}",
          HttpMethod.Delete);

        return NoContent();
    }
}
