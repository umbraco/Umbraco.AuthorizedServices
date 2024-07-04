using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Umbraco.AuthorizedServices.Configuration;
using Umbraco.AuthorizedServices.Services;
using Umbraco.Cms.Api.Common.Attributes;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Cms.Web.Common.Routing;

namespace Umbraco.AuthorizedServices.Api.Management.Controllers.ServiceResponse
{
    [ApiController]
    [BackOfficeRoute($"{Constants.ManagementApi.RootPath}/v{{version:apiVersion}}/service-response")]
    [Authorize(Policy = AuthorizationPolicies.BackOfficeAccess)]
    [MapToApi(Constants.ManagementApi.ApiName)]
    public class AuthorizedServiceResponseControllerBase : Controller
    {
        protected IAuthorizedServiceAuthorizer ServiceAuthorizer { get; }
        protected IOptionsMonitor<ServiceDetail> ServiceDetailOptions { get; }
        protected AppCaches AppCaches { get; }

        public AuthorizedServiceResponseControllerBase(
            IAuthorizedServiceAuthorizer serviceAuthorizer,
            IOptionsMonitor<ServiceDetail> serviceDetailOptions,
            AppCaches appCaches)
        {
            ServiceAuthorizer = serviceAuthorizer;
            ServiceDetailOptions = serviceDetailOptions;
            AppCaches = appCaches;
        }
    }
}
