using Microsoft.Extensions.Options;
using Umbraco.AuthorizedServices.Configuration;
using Umbraco.AuthorizedServices.Services;

namespace Umbraco.AuthorizedServices.Tests.Services;

internal abstract class AuthorizedServiceTestsBase
{
    protected const string ServiceAlias = "testService";

    protected Mock<ITokenStorage> TokenStorageMock { get; set; } = null!;

    protected static Mock<IOptionsMonitor<AuthorizedServiceSettings>> CreateOptionsMonitorSettings()
    {
        var settings = new AuthorizedServiceSettings
        {
            Services = new List<ServiceDetail>
            {
                new ServiceDetail
                {
                    Alias = ServiceAlias,
                    ApiHost = "https://service.url"
                }
            }
        };
        var optionsMonitorMock = new Mock<IOptionsMonitor<AuthorizedServiceSettings>>();
        optionsMonitorMock.Setup(o => o.CurrentValue).Returns(settings);
        return optionsMonitorMock;
    }
}
