using Umbraco.Cms.Api.Management.OpenApi;

namespace Umbraco.AuthorizedServices.Api.Configuration;

public class BackOfficeSecurityRequirementsOperationFilter : BackOfficeSecurityRequirementsOperationFilterBase
{
    protected override string ApiName => Constants.ManagementApi.ApiName;
}
