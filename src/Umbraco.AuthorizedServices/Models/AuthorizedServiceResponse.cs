namespace Umbraco.AuthorizedServices.Models;

/// <summary>
/// Represents a response from a request to an authorized service, returning the expected model deserialized from the response data,
/// along with standard service response data.
/// </summary>
/// <typeparam name="TResponse">The response data type.</typeparam>
public class AuthorizedServiceResponse<TResponse>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AuthorizedServiceResponse{TResponse}"/> class.
    /// </summary>
    public AuthorizedServiceResponse()
    {
        Metadata = new ServiceResponseMetadata();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthorizedServiceResponse{TResponse}"/> class.
    /// </summary>
    /// <param name="metadata">The service response metadata.</param>
    public AuthorizedServiceResponse(ServiceResponseMetadata metadata)
    {
        Metadata = metadata;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthorizedServiceResponse{TResponse}"/> class.
    /// </summary>
    /// <param name="responseData">The deserialized response data.</param>
    /// <param name="responseRaw">The raw JSON response data.</param>
    /// <param name="metadata">The service response metadata.</param>
    public AuthorizedServiceResponse(TResponse responseData, string responseRaw, ServiceResponseMetadata metadata)
    {
        Data = responseData;
        Raw = responseRaw;
        Metadata = metadata;
    }

    /// <summary>
    /// Gets the deserialized model response data.
    /// </summary>
    public TResponse? Data { get; }

    /// <summary>
    /// Gets the raw JSON response data.
    /// </summary>
    public string? Raw { get; }

    /// <summary>
    /// Gets the deserialized service response metadata.
    /// </summary>
    public ServiceResponseMetadata Metadata { get; }
}
