using Microsoft.AspNetCore.Http;
using Umbraco.AuthorizedServices.Configuration;

namespace Umbraco.AuthorizedServices.Services;

public interface IAuthorizationUrlBuilder
{
    string BuildUrl(ServiceDetail serviceDetail, HttpContext httpContext);
}
