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
        RawHeaders = new Dictionary<string, IEnumerable<string>>();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthorizedServiceResponse{TResponse}"/> class.
    /// </summary>
    /// <param name="metadata">The service response metadata.</param>
    /// <param name="rawHeaders">The raw headers.</param>
    public AuthorizedServiceResponse(ServiceResponseMetadata metadata, IDictionary<string, IEnumerable<string>> rawHeaders)
    {
        Metadata = metadata;
        RawHeaders = rawHeaders;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthorizedServiceResponse{TResponse}"/> class.
    /// </summary>
    /// <param name="responseData">The deserialized response data.</param>
    /// <param name="rawResponse">The raw JSON response data.</param>
    /// <param name="metadata">The service response metadata.</param>
    /// <param name="rawHeaders">The raw headers.</param>
    public AuthorizedServiceResponse(TResponse responseData, string rawResponse, ServiceResponseMetadata metadata, IDictionary<string, IEnumerable<string>> rawHeaders)
        : this(metadata, rawHeaders)
    {
        Data = responseData;
        RawResponse = rawResponse;
    }

    /// <summary>
    /// Gets the deserialized model response data.
    /// </summary>
    public TResponse? Data { get; }

    /// <summary>
    /// Gets the raw JSON response data.
    /// </summary>
    public string? RawResponse { get; }

    /// <summary>
    /// Gets the deserialized service response metadata.
    /// </summary>
    public ServiceResponseMetadata Metadata { get; }

    /// <summary>
    /// Gets the raw headers.
    /// </summary>
    public IDictionary<string, IEnumerable<string>> RawHeaders { get; }
}
