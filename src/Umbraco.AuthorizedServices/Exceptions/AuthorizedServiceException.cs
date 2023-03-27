namespace Umbraco.AuthorizedServices.Exceptions
{
    /// <summary>
    /// Defines a custom exception thrown when an error occurs when working with an authorized service.
    /// </summary>
    public class AuthorizedServiceException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AuthorizedServiceException"/> class.
        /// </summary>
        public AuthorizedServiceException(string message)
            : base(message)
        {
        }
    }
}
