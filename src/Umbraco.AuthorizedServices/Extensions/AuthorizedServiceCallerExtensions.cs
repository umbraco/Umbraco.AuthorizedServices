using Umbraco.AuthorizedServices.Services;

namespace Umbraco.AuthorizedServices.Extensions
{
    /// <summary>
    /// Provides extensions to <see cref="IAuthorizedServiceCaller"/>.
    /// </summary>
    public static class AuthorizedServiceCallerExtensions
    {
        /// <summary>
        /// Sends a request to an authorized service using the GET HTTP method.
        /// </summary>
        /// <typeparam name="TResponse">The response type.</typeparam>
        /// <param name="caller">The <see cref="IAuthorizedServiceCaller"/> instance.</param>
        /// <param name="serviceAlias">The service alias.</param>
        /// <param name="path">The request path.</param>
        /// <returns>A <see cref="Task{TResponse}"/> representing the result of the asynchronous operation.</returns>
        public static async Task<TResponse> GetRequestAsync<TResponse>(this IAuthorizedServiceCaller caller, string serviceAlias, string path) =>
            await caller.SendRequestAsync<TResponse>(serviceAlias, path, HttpMethod.Get);

        /// <summary>
        /// Sends a request to an authorized service using the GET POST method.
        /// </summary>
        /// <typeparam name="TResponse">The response type.</typeparam>
        /// <typeparam name="TRequest">The request type.</typeparam>
        /// <param name="caller">The <see cref="IAuthorizedServiceCaller"/> instance.</param>
        /// <param name="serviceAlias">The service alias.</param>
        /// <param name="path">The request path.</param>
        /// <param name="requestContent">The typed request data.</param>
        /// <returns>A <see cref="Task{TResponse}"/> representing the result of the asynchronous operation.</returns>
        public static async Task<TResponse> PostRequestAsync<TRequest, TResponse>(this IAuthorizedServiceCaller caller, string serviceAlias, string path, TRequest? requestContent = null) where TRequest : class =>
            await caller.SendRequestAsync<TRequest, TResponse>(serviceAlias, path, HttpMethod.Post, requestContent);

        /// <summary>
        /// Sends a request to an authorized service using the GET PUT method.
        /// </summary>
        /// <typeparam name="TResponse">The response type.</typeparam>
        /// <typeparam name="TRequest">The request type.</typeparam>
        /// <param name="caller">The <see cref="IAuthorizedServiceCaller"/> instance.</param>
        /// <param name="serviceAlias">The service alias.</param>
        /// <param name="path">The request path.</param>
        /// <param name="requestContent">The typed request data.</param>
        /// <returns>A <see cref="Task{TResponse}"/> representing the result of the asynchronous operation.</returns>
        public static async Task<TResponse> PutRequestAsync<TRequest, TResponse>(this IAuthorizedServiceCaller caller, string serviceAlias, string path, TRequest? requestContent = null) where TRequest : class =>
            await caller.SendRequestAsync<TRequest, TResponse>(serviceAlias, path, HttpMethod.Put, requestContent);

        /// <summary>
        /// Sends a request to an authorized service using the GET DELETE method.
        /// </summary>
        /// <typeparam name="TResponse">The response type.</typeparam>
        /// <param name="caller">The <see cref="IAuthorizedServiceCaller"/> instance.</param>
        /// <param name="serviceAlias">The service alias.</param>
        /// <param name="path">The request path.</param>
        /// <returns>A <see cref="Task{TResponse}"/> representing the result of the asynchronous operation.</returns>
        public static async Task<TResponse> DeleteRequestAsync<TResponse>(this IAuthorizedServiceCaller caller, string serviceAlias, string path) =>
            await caller.SendRequestAsync<TResponse>(serviceAlias, path, HttpMethod.Delete);
    }
}
