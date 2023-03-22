using System.Net;

namespace Umbraco.AuthorizedServices.Exceptions
{
    public class AuthorizedServiceHttpException : AuthorizedServiceException
    {
        public AuthorizedServiceHttpException(string message, HttpStatusCode statusCode, string? reason, string? content)
            : base(message)
        {
            StatusCode = statusCode;
            Reason = reason;
            Content = content;
        }

        public HttpStatusCode StatusCode { get; set; }

        public string? Reason { get; set; }

        public string? Content { get; set; }
    }
}
