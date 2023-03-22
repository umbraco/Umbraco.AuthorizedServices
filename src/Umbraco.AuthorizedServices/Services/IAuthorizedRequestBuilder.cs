using Umbraco.AuthorizedServices.Configuration;
using Umbraco.AuthorizedServices.Models;

namespace Umbraco.AuthorizedServices.Services;

public interface IAuthorizedRequestBuilder
{
    HttpRequestMessage CreateRequestMessage<TRequest>(
        ServiceDetail serviceDetail,
        string path,
        HttpMethod httpMethod,
        Token token,
        TRequest? requestContent)
        where TRequest : class;
}
