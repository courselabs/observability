using Microsoft.Extensions.Configuration;

namespace Fulfilment.Core.Configuration
{
    public static class ConfigurationBuilderExtensions
    {
        public static IConfigurationBuilder AddStandardSources(this IConfigurationBuilder builder)
        {
            return builder.AddJsonFile("appsettings.json")
                          .AddEnvironmentVariables()
                          .AddJsonFile("config/override.json", optional: true, reloadOnChange: true);
        }
    }
}
