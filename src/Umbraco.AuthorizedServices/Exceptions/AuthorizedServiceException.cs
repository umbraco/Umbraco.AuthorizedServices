namespace Umbraco.AuthorizedServices.Exceptions
{
    public class AuthorizedServiceException : Exception
    {
        public AuthorizedServiceException(string message)
            : base(message)
        {
        }
    }
}
