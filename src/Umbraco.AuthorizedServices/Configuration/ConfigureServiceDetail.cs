using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Umbraco.AuthorizedServices.Configuration;

internal sealed class ConfigureServiceDetail : IConfigureNamedOptions<ServiceDetail>
{
    private readonly IConfiguration _configuration;

    public ConfigureServiceDetail(IConfiguration configuration)
        => _configuration = configuration;

    public void Configure(string name, ServiceDetail options)
    {
        // Ensure the configuration section exists
        IConfigurationSection serviceSection = _configuration.GetSection($"Umbraco:AuthorizedServices:Services:{name}");
        if (!serviceSection.Exists())
        {
            throw new InvalidOperationException($"Cannot find service config for service alias '{name}'");
        }

        // Bind section to options instance and set the alias
        serviceSection.Bind(options);
        options.Alias = name;
    }

    public void Configure(ServiceDetail options) => Configure(Options.DefaultName, options);
}
