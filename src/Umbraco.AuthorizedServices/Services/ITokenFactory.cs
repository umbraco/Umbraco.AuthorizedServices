using Umbraco.AuthorizedServices.Configuration;
using Umbraco.AuthorizedServices.Models;

namespace Umbraco.AuthorizedServices.Services;

public interface ITokenFactory
{
    Token CreateFromResponseContent(string responseContent, string serviceAlias, ServiceDetail serviceDetail);
}
