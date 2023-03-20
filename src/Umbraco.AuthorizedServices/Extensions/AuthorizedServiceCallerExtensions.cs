using Umbraco.AuthorizedServices.Services;

namespace Umbraco.AuthorizedServices.Extensions
{
    public static class AuthorizedServiceCallerExtensions
    {
        public static async Task<TResponse> GetRequestAsync<TResponse>(this IAuthorizedServiceCaller caller, string serviceAlias, string path) =>
            await caller.SendRequestAsync<TResponse>(serviceAlias, path, HttpMethod.Get);

        public static async Task<TResponse> PostRequestAsync<TRequest, TResponse>(this IAuthorizedServiceCaller caller, string serviceAlias, string path, TRequest? requestContent = null) where TRequest : class =>
            await caller.SendRequestAsync<TRequest, TResponse>(serviceAlias, path, HttpMethod.Post, requestContent);

        public static async Task<TResponse> PutRequestAsync<TRequest, TResponse>(this IAuthorizedServiceCaller caller, string serviceAlias, string path, TRequest? requestContent = null) where TRequest : class =>
            await caller.SendRequestAsync<TRequest, TResponse>(serviceAlias, path, HttpMethod.Put, requestContent);

        public static async Task<TResponse> DeleteRequestAsync<TResponse>(this IAuthorizedServiceCaller caller, string serviceAlias, string path) =>
            await caller.SendRequestAsync<TResponse>(serviceAlias, path, HttpMethod.Delete);
    }
}
