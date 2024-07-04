using System.ComponentModel.DataAnnotations;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Umbraco.AuthorizedServices.Configuration;
using Umbraco.Cms.Api.Common.Attributes;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Cms.Web.Common.Routing;

namespace Umbraco.AuthorizedServices.Controllers.Tree;

[ApiController]
[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = "Tree")]
[BackOfficeRoute($"{Constants.ManagementApi.RootPath}/v{{version:apiVersion}}/tree")]
[Authorize(Policy = AuthorizationPolicies.BackOfficeAccess)]
[MapToApi(Constants.ManagementApi.ApiName)]
public class ChildrenAuthorizedServiceTreeController : ControllerBase
{
    private readonly IOptionsMonitor<AuthorizedServiceSettings> _authorizedServiceSettings;

    public ChildrenAuthorizedServiceTreeController(IOptionsMonitor<AuthorizedServiceSettings> authorizedServiceSettings) =>
        _authorizedServiceSettings = authorizedServiceSettings;

    [HttpGet("children")]
    [ProducesResponseType(typeof(PagedViewModel<AuthorizedServiceTreeItemResponseModel>), StatusCodes.Status200OK)]
    public IActionResult Children(Guid parentId, int skip = 0, int take = 100)
    {
        IEnumerable<AuthorizedServiceTreeItemResponseModel> treeItems = _authorizedServiceSettings.CurrentValue.Services.Values.OrderBy(x => x.DisplayName)
            .Skip(skip)
            .Take(take)
            .Select(x => new AuthorizedServiceTreeItemResponseModel()
            {
                Icon = x.Icon,
                Name = x.DisplayName,
                Unique = x.Alias,
            });

        return Ok(new PagedViewModel<AuthorizedServiceTreeItemResponseModel>()
        {
            Items = treeItems,
            Total = _authorizedServiceSettings.CurrentValue.Services.Count,
        });
    }
}

public sealed record AuthorizedServiceTreeItemResponseModel
{
    public required string Unique { get; set; }
    public required string Icon { get; set; }
    public required string Name { get; set; }
}

public class PagedViewModel<T>
{
    [Required]
    public long Total { get; set; }

    [Required]
    public IEnumerable<T> Items { get; set; } = [];
}
