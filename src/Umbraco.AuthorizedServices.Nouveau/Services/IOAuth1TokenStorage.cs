using Umbraco.AuthorizedServices.Models;

namespace Umbraco.AuthorizedServices.Services;

public interface IOAuth1TokenStorage : ITokenStorage<OAuth1Token>
{
}
