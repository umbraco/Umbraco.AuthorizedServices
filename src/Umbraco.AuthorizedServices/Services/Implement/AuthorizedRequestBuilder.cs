using System.Net.Http.Headers;
using System.Text;
using Umbraco.AuthorizedServices.Configuration;
using Umbraco.AuthorizedServices.Models;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.AuthorizedServices.Services.Implement;

internal sealed class AuthorizedRequestBuilder : IAuthorizedRequestBuilder
{
    private readonly IJsonSerializer _jsonSerializer;

    public AuthorizedRequestBuilder(IJsonSerializer jsonSerializer) => _jsonSerializer = jsonSerializer;

    public HttpRequestMessage CreateRequestMessage<TRequest>(ServiceDetail serviceDetail, string path, HttpMethod httpMethod, Token token, TRequest? requestContent)
        where TRequest : class
    {
        var requestMessage = new HttpRequestMessage
        {
            Method = httpMethod,
            RequestUri = new Uri(serviceDetail.ApiHost + path),
            Content = GetRequestContent(requestContent)
        };

        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);
        requestMessage.Headers.UserAgent.Add(new ProductInfoHeaderValue("UmbracoServiceIntegration", "1.0.0"));
        return requestMessage;
    }

    private StringContent? GetRequestContent<TRequest>(TRequest? requestContent)
    {
        if (requestContent == null)
        {
            return null;
        }

        var serializedContent = _jsonSerializer.Serialize(requestContent);
        return new StringContent(serializedContent, Encoding.UTF8, "application/json");
    }
}
