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
    private readonly AuthorizedServiceSettings _authorizedServiceSettings;
    private readonly IJsonSerializer _jsonSerializer;

    public JsonSerializerFactory(IOptionsMonitor<AuthorizedServiceSettings> authorizedServiceSettings, IJsonSerializer jsonSerializer)
    {
        _authorizedServiceSettings = authorizedServiceSettings.CurrentValue;
        _jsonSerializer = jsonSerializer;
    }

    /// <summary>
    /// Instantiates the appropriate JSON serializer for the provided service alias.
    /// </summary>
    /// <param name="serviceAlias">The service alias.</param>
    /// <returns>An implementation of <see cref="IJsonSerializer"/>.</returns>
    public IJsonSerializer GetSerializer(string serviceAlias)
    {
        ServiceDetail? serviceDetail = _authorizedServiceSettings.Services.SingleOrDefault(x => x.Alias == serviceAlias);
        if (serviceDetail == null)
        {
            throw new ArgumentException(nameof(serviceAlias), $"Could not find a registered service with the provided alias of {serviceAlias}");
        }

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
