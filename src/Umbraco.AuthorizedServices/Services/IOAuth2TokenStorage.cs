using Umbraco.AuthorizedServices.Models;

namespace Umbraco.AuthorizedServices.Services;

public interface IOAuth2TokenStorage : ITokenStorage<OAuth2Token>
{
}
