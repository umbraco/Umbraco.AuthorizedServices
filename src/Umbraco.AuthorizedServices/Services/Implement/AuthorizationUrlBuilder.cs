using System.Text;
using Microsoft.AspNetCore.Http;
using Umbraco.AuthorizedServices.Configuration;
using Umbraco.AuthorizedServices.Extensions;

namespace Umbraco.AuthorizedServices.Services.Implement;

internal sealed class AuthorizationUrlBuilder : IAuthorizationUrlBuilder
{
    public string BuildUrl(ServiceDetail serviceDetail, HttpContext httpContext)
    {
        var url = new StringBuilder();
        url.Append(serviceDetail.IdentityHost);
        url.Append(serviceDetail.RequestIdentityPath);
        url.Append("?client_id=").Append(serviceDetail.ClientId);

        if (serviceDetail.AuthorizationRequestsRequireRedirectUri)
        {
            url.Append("&redirect_uri=").Append(httpContext.GetAuthorizedServiceRedirectUri());
        }

        url.Append("&scope=").Append(serviceDetail.Scopes);

        url.Append("&response_type=code");

        url.Append("&response_mode=query");

        url.Append("&state=").Append(serviceDetail.Alias + "|" + Constants.Authorization.State);

        return url.ToString();
    }
}
