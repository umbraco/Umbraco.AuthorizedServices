using System.Collections.Specialized;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using Umbraco.AuthorizedServices.Configuration;
using Umbraco.AuthorizedServices.Helpers;
using Umbraco.AuthorizedServices.Models;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.AuthorizedServices.Services.Implement;

internal sealed class AuthorizedRequestBuilder : IAuthorizedRequestBuilder
{
    private readonly JsonSerializerFactory _jsonSerializerFactory;

    public AuthorizedRequestBuilder(JsonSerializerFactory jsonSerializerFactory) => _jsonSerializerFactory = jsonSerializerFactory;

    public HttpRequestMessage CreateRequestMessageWithOAuth2Token<TRequest>(
        ServiceDetail serviceDetail,
        string path,
        HttpMethod httpMethod,
        OAuth2Token token,
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
       string apiKey,
       TRequest? requestContent)
       where TRequest : class
    {
        if (string.IsNullOrWhiteSpace(apiKey))
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
            requestUri = new Uri($"{requestUri}{(queryStringParams.Count > 0 ? "&" : "?")}{serviceDetail.ApiKeyProvision.Key}={apiKey}");
        }

        HttpRequestMessage requestMessage = CreateRequestMessage(
            httpMethod,
            requestUri,
            GetRequestContent(serviceDetail, requestContent));

        if (serviceDetail.ApiKeyProvision.Method == ApiKeyProvisionMethod.HttpHeader)
        {
            requestMessage.Headers.Add(serviceDetail.ApiKeyProvision.Key, apiKey);
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

    public HttpRequestMessage CreateRequestMessageWithOAuth1Token<TRequest>(ServiceDetail serviceDetail, string path, HttpMethod httpMethod, OAuth1Token oauth1Token, TRequest? requestContent) where TRequest : class
    {
        var url = serviceDetail.ApiHost + path;

        var nonce = OAuth1Helper.GetNonce();
        var timestamp = OAuth1Helper.GetTimestamp();

        var authorizationParams =
            new Dictionary<string, string>
            {
                { "oauth_consumer_key", serviceDetail.ClientId },
                { "oauth_nonce", nonce },
                { "oauth_signature_method", "HMAC-SHA1" },
                { "oauth_timestamp", timestamp },
                { "oauth_token", oauth1Token.OAuthToken },
                { "oauth_version", "1.0" }
            };

        var signature = GetAuthorizedSignature(
            httpMethod.Method.ToUpper(),
            url,
            serviceDetail.ClientSecret,
            oauth1Token,
            authorizationParams);

        url += "?oauth_consumer_key=" + serviceDetail.ClientId
            + "&oauth_nonce=" + nonce
            + "&oauth_signature=" + Uri.EscapeDataString(signature)
            + "&oauth_signature_method=HMAC-SHA1"
            + "&oauth_timestamp=" + timestamp
            + "&oauth_token=" + oauth1Token.OAuthToken
            + "&oauth_version=1.0";

        HttpRequestMessage requestMessage = CreateRequestMessage(
            httpMethod,
            new Uri(url),
            GetRequestContent(serviceDetail, requestContent));
        AddCommonHeaders(requestMessage);
        return requestMessage;
    }

    private string GetAuthorizedSignature(
       string httpMethod,
       string url,
       string consumerSecret,
       OAuth1Token oauth1Token,
       Dictionary<string, string> authorizationParams)
    {
        string hashingKey = string.Format("{0}&{1}", consumerSecret, oauth1Token.OAuthTokenSecret);

        using var hasher = new HMACSHA1(new ASCIIEncoding().GetBytes(hashingKey));

        string authorizationParamsStr = string.Join(
            "&",
            authorizationParams
                .Select(kvp => string.Format("{0}={1}", Uri.EscapeDataString(kvp.Key), Uri.EscapeDataString(kvp.Value)))
                .OrderBy(p => p));

        // signature format: HTTP method (uppercase) + & + request URL + & + authorization parameters
        string signature = string.Format(
            "{0}&{1}&{2}",
            httpMethod,
            Uri.EscapeDataString(url),
            Uri.EscapeDataString(authorizationParamsStr));

        return Convert.ToBase64String(hasher.ComputeHash(new ASCIIEncoding().GetBytes(signature)));
    }
}
