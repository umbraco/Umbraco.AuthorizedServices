using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Infrastructure.DependencyInjection;

namespace Umbraco.Licenses.TestSite;

/// <summary>
/// License validation only runs on servers with Single or SchedulingPublisher server role.
/// </summary>
public class TestSiteComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder) => _ = builder.SetServerRegistrar<SingleServerRoleAccessor>();
}

public class SingleServerRoleAccessor : IServerRoleAccessor
{
    public ServerRole CurrentServerRole => ServerRole.Single;
}
