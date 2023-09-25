using Microsoft.Extensions.Options;
using Umbraco.AuthorizedServices.Configuration;
using Umbraco.AuthorizedServices.Services;

namespace Umbraco.AuthorizedServices.Tests.Services;

internal abstract class AuthorizedServiceTestsBase
{
    protected const string ServiceAlias = "testService";

    protected Mock<ITokenStorage> TokenStorageMock { get; set; } = null!;

    protected Mock<IKeyStorage> KeyStorageMock { get; set; } = null!;

    protected static Mock<IOptionsMonitor<ServiceDetail>> CreateOptionsMonitorServiceDetail(
        AuthenticationMethod authenticationMethod = AuthenticationMethod.OAuth2AuthorizationCode)
    {
        var optionsMonitorServiceDetailMock = new Mock<IOptionsMonitor<ServiceDetail>>();
        optionsMonitorServiceDetailMock.Setup(o => o.Get(ServiceAlias)).Returns(new ServiceDetail()
        {
            Alias = ServiceAlias,
            ApiHost = "https://service.url",
            AuthenticationMethod = authenticationMethod,
            JsonSerializer = JsonSerializerOption.JsonNet,
            ApiKey = authenticationMethod == AuthenticationMethod.ApiKey ? "test-api-key" : string.Empty,
            ApiKeyProvision = authenticationMethod == AuthenticationMethod.ApiKey
                ? new ApiKeyProvision { Method = ApiKeyProvisionMethod.QueryString, Key = "key"} : null
        });

        return optionsMonitorServiceDetailMock;
    }
}
