using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Umbraco.AuthorizedServices.Configuration;
using Umbraco.Cms.Api.Common.Attributes;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Cms.Web.Common.Routing;

namespace Umbraco.AuthorizedServices.Api.Management.Controllers.Service
{
    [ApiController]
    [BackOfficeRoute($"{Constants.ManagementApi.RootPath}/v{{version:apiVersion}}/service")]
    [Authorize(Policy = AuthorizationPolicies.BackOfficeAccess)]
    [MapToApi(Constants.ManagementApi.ApiName)]
    public class AuthorizedServiceControllerBase : Controller
    {
        protected IOptionsMonitor<ServiceDetail> ServiceDetailOptions { get; }

        public AuthorizedServiceControllerBase(IOptionsMonitor<ServiceDetail> serviceDetailOptions) => ServiceDetailOptions = serviceDetailOptions;
    }
}
