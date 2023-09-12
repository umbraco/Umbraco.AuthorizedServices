using System.Collections.Specialized;
using System.Net.Http.Headers;
using System.Text;
using System.Web;
using Umbraco.AuthorizedServices.Configuration;
using Umbraco.AuthorizedServices.Models;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.AuthorizedServices.Services.Implement;

internal sealed class AuthorizedRequestBuilder : IAuthorizedRequestBuilder
{
    private readonly JsonSerializerFactory _jsonSerializerFactory;

    public AuthorizedRequestBuilder(JsonSerializerFactory jsonSerializerFactory) => _jsonSerializerFactory = jsonSerializerFactory;

    public HttpRequestMessage CreateRequestMessageWithToken<TRequest>(
        ServiceDetail serviceDetail,
        string path,
        HttpMethod httpMethod,
        Token token,
        TRequest? requestContent)
        where TRequest : class
    {
        var requestMessage = new HttpRequestMessage
        {
            Method = httpMethod,
            RequestUri = new Uri(serviceDetail.ApiHost + path),
            Content = GetRequestContent(serviceDetail, requestContent)
        };

        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);
        requestMessage.Headers.UserAgent.Add(new ProductInfoHeaderValue("UmbracoServiceIntegration", "1.0.0"));
        requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        return requestMessage;
    }

    public HttpRequestMessage CreateRequestMessageWithApiKey<TRequest>(
       ServiceDetail serviceDetail,
       string path,
       HttpMethod httpMethod,
       TRequest? requestContent)
       where TRequest : class
    {
        var requestUri = new Uri(serviceDetail.ApiHost + path);
        if (serviceDetail.ApiKeyProvision is not null && serviceDetail.ApiKeyProvision.Method == ApiKeyProvisionMethod.QueryString)
        {
            NameValueCollection queryStringParams = HttpUtility.ParseQueryString(requestUri.Query);

            requestUri = new Uri($"{requestUri}{(queryStringParams.Count > 0 ? "&" : "?")}{serviceDetail.ApiKeyProvision.Key}={serviceDetail.ApiKey}");
        }

        var requestMessage = new HttpRequestMessage
        {
            Method = httpMethod,
            RequestUri= requestUri,
            Content = GetRequestContent(serviceDetail, requestContent)
        };

        if (serviceDetail.ApiKeyProvision is not null && serviceDetail.ApiKeyProvision.Method == ApiKeyProvisionMethod.HttpHeader)
        {
            requestMessage.Headers.Add(serviceDetail.ApiKeyProvision.Key, serviceDetail.ApiKey);
        }

        requestMessage.Headers.UserAgent.Add(new ProductInfoHeaderValue("UmbracoServiceIntegration", "1.0.0"));
        return requestMessage;
    }

    private StringContent? GetRequestContent<TRequest>(ServiceDetail serviceDetail, TRequest? requestContent)
    {
        if (requestContent == null)
        {
            return null;
        }

        IJsonSerializer jsonSerializer = _jsonSerializerFactory.GetSerializer(serviceDetail.Alias);
        var serializedContent = jsonSerializer.Serialize(requestContent);
        return new StringContent(serializedContent, Encoding.UTF8, "application/json");
    }
}
