using Microsoft.Extensions.Options;
using Umbraco.AuthorizedServices.Configuration;
using Umbraco.AuthorizedServices.Services.Implement;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Infrastructure.Serialization;

namespace Umbraco.AuthorizedServices.Services;

/// <summary>
/// Defines a factory for retrieving the appropriate JSON serializer for an authorized service.
/// </summary>
public class JsonSerializerFactory
{
    private readonly IOptionsMonitor<ServiceDetail> _serviceDetailOptions;
    private readonly IJsonSerializer _jsonSerializer;

    public JsonSerializerFactory(IOptionsMonitor<ServiceDetail> serviceDetailOptions, IJsonSerializer jsonSerializer)
    {
        _serviceDetailOptions = serviceDetailOptions;
        _jsonSerializer = jsonSerializer;
    }

    /// <summary>
    /// Instantiates the appropriate JSON serializer for the provided service alias.
    /// </summary>
    /// <param name="serviceAlias">The service alias.</param>
    /// <returns>An implementation of <see cref="IJsonSerializer"/>.</returns>
    public IJsonSerializer GetSerializer(string serviceAlias)
    {
        ServiceDetail serviceDetail = _serviceDetailOptions.Get(serviceAlias);

        switch (serviceDetail.JsonSerializer)
        {
            case JsonSerializerOption.Default:
                return _jsonSerializer;
            case JsonSerializerOption.JsonNet:
                return new JsonNetSerializer();
            case JsonSerializerOption.SystemTextJson:
                return new SystemTextJsonSerializer();
            default:
                throw new ArgumentOutOfRangeException(serviceDetail.JsonSerializer.ToString());
        }
    }
}
