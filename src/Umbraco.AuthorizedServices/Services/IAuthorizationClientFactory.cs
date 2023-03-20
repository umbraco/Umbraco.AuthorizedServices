namespace Umbraco.AuthorizedServices.Services;

public interface IAuthorizationClientFactory
{
    HttpClient CreateClient();
}
