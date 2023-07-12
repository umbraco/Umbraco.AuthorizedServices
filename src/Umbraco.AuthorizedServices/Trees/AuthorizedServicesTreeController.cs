using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Umbraco.AuthorizedServices.Configuration;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models.Trees;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Trees;
using Umbraco.Cms.Web.BackOffice.Trees;
using Umbraco.Cms.Web.Common.Attributes;
using Umbraco.Cms.Web.Common.ModelBinders;

namespace Umbraco.AuthorizedServices.Trees;

/// <summary>
/// Defines an Umbraco <see cref="TreeController"/> for access to the configured authorized services.
/// </summary>
[Authorize(Policy = Cms.Web.Common.Authorization.AuthorizationPolicies.SectionAccessSettings)]
[Tree(Cms.Core.Constants.Applications.Settings, Constants.Trees.AuthorizedServices, TreeTitle = "Authorized Services")]
[PluginController(Constants.PluginName)]
public class AuthorizedServicesTreeController : TreeController
{
    private readonly IMenuItemCollectionFactory _menuItemCollectionFactory;
    private readonly IOptionsMonitor<AuthorizedServiceSettings> _authorizedServiceSettings;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthorizedServicesTreeController"/> class.
    /// </summary>
    public AuthorizedServicesTreeController(
        ILocalizedTextService textService,
        UmbracoApiControllerTypeCollection umbracoApiControllerTypeCollection,
        IEventAggregator eventAggregator,
        IMenuItemCollectionFactory menuItemCollectionFactory,
        IOptionsMonitor<AuthorizedServiceSettings> authorizedServiceSettings)
        : base(textService, umbracoApiControllerTypeCollection, eventAggregator)
    {
        _menuItemCollectionFactory = menuItemCollectionFactory;
        _authorizedServiceSettings = authorizedServiceSettings;
    }

    /// <inheritdoc/>
    protected override ActionResult<MenuItemCollection> GetMenuForNode(string id, [ModelBinder(typeof(HttpQueryStringModelBinder))] FormCollection queryStrings)
    {
        MenuItemCollection menu = _menuItemCollectionFactory.Create();

        if (id == Cms.Core.Constants.System.RootString)
        {
            menu.Items.Add(new RefreshNode(LocalizedTextService, true));
        }

        return menu;
    }

    /// <inheritdoc/>
    protected override ActionResult<TreeNodeCollection> GetTreeNodes(string id, FormCollection queryStrings)
    {
        var nodes = new TreeNodeCollection();
        foreach (ServiceSummary? service in _authorizedServiceSettings.CurrentValue.Services.Values.OrderBy(x => x.DisplayName))
        {
            TreeNode node = CreateTreeNode(service.Alias, "-1", queryStrings, service.DisplayName, service.Icon, false);
            nodes.Add(node);
        }

        return nodes;
    }
}
