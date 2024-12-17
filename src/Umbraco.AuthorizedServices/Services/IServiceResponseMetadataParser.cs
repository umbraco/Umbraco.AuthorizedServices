using Umbraco.AuthorizedServices.Models;

namespace Umbraco.AuthorizedServices.Services;

/// <summary>
/// Defines operations on parsing the metadata from an authorized service response.
/// </summary>
public interface IServiceResponseMetadataParser
{
    /// <summary>
    /// Creates an HTTP client for use in authorization operations.
    /// </summary>
    ServiceResponseMetadata ParseMetadata(HttpResponseMessage httpResponseMessage);
}
