using System.Net;

namespace Umbraco.AuthorizedServices.Exceptions
{
    /// <summary>
    /// Defines a custom exception thrown when an HTTP related error occurs when working with an authorized service.
    /// </summary>
    public class AuthorizedServiceHttpException : AuthorizedServiceException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AuthorizedServiceHttpException"/> class.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="statusCode">The HTTP status code.</param>
        /// <param name="reason">The HTTP error reason phrase</param>
        /// <param name="content">The exception content.</param>
        public AuthorizedServiceHttpException(string message, HttpStatusCode statusCode, string? reason, string? content)
            : base(message)
        {
            StatusCode = statusCode;
            Reason = reason;
            Content = content;
        }

        /// <summary>
        /// Gets the HTTP status code.
        /// </summary>
        public HttpStatusCode StatusCode { get; }

        /// <summary>
        /// Gets the HTTP error reason phrase.
        /// </summary>
        public string? Reason { get; }

        /// <summary>
        /// Gets the exception content.
        /// </summary>
        public string? Content { get; }
    }
}
