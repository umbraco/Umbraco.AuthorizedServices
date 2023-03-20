namespace Umbraco.AuthorizedServices.Services;

public interface IAuthorizedServiceCaller
{
    Task<TResponse> SendRequestAsync<TResponse>(string serviceAlias, string path, HttpMethod httpMethod);

    Task<TResponse> SendRequestAsync<TRequest, TResponse>(string serviceAlias, string path, HttpMethod httpMethod, TRequest? requestContent = null)
        where TRequest : class;

    Task<string> SendRequestRawAsync(string serviceAlias, string path, HttpMethod httpMethod);

    Task<string> SendRequestRawAsync<TRequest>(string serviceAlias, string path, HttpMethod httpMethod, TRequest? requestContent = null)
        where TRequest : class;
}
