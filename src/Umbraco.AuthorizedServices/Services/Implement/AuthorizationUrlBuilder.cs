using System.Net;
using System.Text;
using Microsoft.AspNetCore.Http;
using Umbraco.AuthorizedServices.Configuration;
using Umbraco.AuthorizedServices.Extensions;

namespace Umbraco.AuthorizedServices.Services.Implement;

internal sealed class AuthorizationUrlBuilder : IAuthorizationUrlBuilder
{
    public string BuildUrl(ServiceDetail serviceDetail, HttpContext httpContext, string state, string codeChallenge)
    {
        var url = new StringBuilder();
        url.Append(serviceDetail.IdentityHost);
        url.Append(serviceDetail.RequestIdentityPath);
        url.Append("?response_type=code");
        url.Append("&client_id=").Append(serviceDetail.ClientId);
        url.Append("&response_mode=query");

        if (serviceDetail.AuthorizationRequestsRequireRedirectUri)
        {
            url.Append("&redirect_uri=").Append(httpContext.GetAuthorizedServiceRedirectUri());
        }

        url.Append("&scope=").Append(serviceDetail.Scopes);

        url.Append("&state=").Append(serviceDetail.Alias + WebUtility.UrlEncode("|") + state);

        if (serviceDetail.UseProofKeyForCodeExchange)
        {
            url.Append("&code_challenge=").Append(codeChallenge);
            url.Append("&code_challenge_method=S256");
        }

        return url.ToString();
    }
}
