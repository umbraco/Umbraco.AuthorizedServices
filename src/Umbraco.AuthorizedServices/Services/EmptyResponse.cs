using Umbraco.Cms.Core;

namespace Umbraco.AuthorizedServices.Services
{
    /// <summary>
    /// Defines an empty response used in service calls that aren't expected to return any data.
    /// </summary>
    /// <remarks>
    /// We need to create this in order to use Umbraco's <see cref="Attempt"/> construct, which requires a type parameter.
    /// </remarks>
    public class EmptyResponse
    {
    }
}
