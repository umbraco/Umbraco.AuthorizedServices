using System.Collections.Specialized;
using System.IO;
using System.Net.Http.Headers;
using System.Text;
using System.Web;
using Azure.Core;
using Microsoft.Extensions.Hosting;
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
        HttpRequestMessage requestMessage = CreateRequestMessage(
            httpMethod,
            new Uri(serviceDetail.ApiHost + path),
            GetRequestContent(serviceDetail, requestContent));
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);
        AddCommonHeaders(requestMessage);
        return requestMessage;
    }

    public HttpRequestMessage CreateRequestMessageWithApiKey<TRequest>(
       ServiceDetail serviceDetail,
       string path,
       HttpMethod httpMethod,
       TRequest? requestContent)
       where TRequest : class
    {
        if (string.IsNullOrWhiteSpace(serviceDetail.ApiKey))
        {
            throw new InvalidOperationException("Cannot create an HTTP request message for an API key request as no API key is available in configuration.");
        }

        if (serviceDetail.ApiKeyProvision is null)
        {
            throw new InvalidOperationException("Cannot create an HTTP request message for an API key request as no API key provision detail has been provided in configuration.");
        }

        var requestUri = new Uri(serviceDetail.ApiHost + path);
        if (serviceDetail.ApiKeyProvision.Method == ApiKeyProvisionMethod.QueryString)
        {
            NameValueCollection queryStringParams = HttpUtility.ParseQueryString(requestUri.Query);
            requestUri = new Uri($"{requestUri}{(queryStringParams.Count > 0 ? "&" : "?")}{serviceDetail.ApiKeyProvision.Key}={serviceDetail.ApiKey}");
        }

        HttpRequestMessage requestMessage = CreateRequestMessage(
            httpMethod,
            requestUri,
            GetRequestContent(serviceDetail, requestContent));

        if (serviceDetail.ApiKeyProvision.Method == ApiKeyProvisionMethod.HttpHeader)
        {
            requestMessage.Headers.Add(serviceDetail.ApiKeyProvision.Key, serviceDetail.ApiKey);
        }

        AddCommonHeaders(requestMessage);
        return requestMessage;
    }

    private HttpRequestMessage CreateRequestMessage(HttpMethod httpMethod, Uri requestUri, HttpContent? content) =>
        new HttpRequestMessage
        {
            Method = httpMethod,
            RequestUri = requestUri,
            Content = content
        };

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

    private static void AddCommonHeaders(HttpRequestMessage requestMessage)
    {
        requestMessage.Headers.UserAgent.Add(new ProductInfoHeaderValue("UmbracoServiceIntegration", "1.0.0"));
        requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }
}
