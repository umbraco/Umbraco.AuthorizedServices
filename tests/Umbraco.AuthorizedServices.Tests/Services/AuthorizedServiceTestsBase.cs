using Microsoft.Extensions.Options;
using Umbraco.AuthorizedServices.Configuration;
using Umbraco.AuthorizedServices.Services;

namespace Umbraco.AuthorizedServices.Tests.Services;

internal abstract class AuthorizedServiceTestsBase
{
    protected const string ServiceAlias = "testService";

    protected Mock<ITokenStorage> TokenStorageMock { get; set; } = null!;

    protected static Mock<IOptionsMonitor<ServiceDetail>> CreateOptionsMonitorServiceDetail()
    {
        var optionsMonitorServiceDetailMock = new Mock<IOptionsMonitor<ServiceDetail>>();
        optionsMonitorServiceDetailMock.Setup(o => o.Get(ServiceAlias)).Returns(new ServiceDetail()
        {
            Alias = ServiceAlias,
            ApiHost = "https://service.url",
            JsonSerializer = JsonSerializerOption.JsonNet
        });

        return optionsMonitorServiceDetailMock;
    }
}
