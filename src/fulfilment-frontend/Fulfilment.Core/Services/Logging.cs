using Fulfilment.Core.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Templates;

namespace Fulfilment.Core.Services
{
    public static class Logging
    {
        public static IServiceCollection AddLogging(this IServiceCollection services, IConfiguration config, Configuration.TraceOptions options)
        {
            var logger = CreateLogger(config, options);
            return services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(logger));
        }
        
        private static Serilog.Core.Logger CreateLogger(IConfiguration config, Configuration.TraceOptions options)
        {
            var loggerConfig = new LoggerConfiguration().ReadFrom.Configuration(config);
            var formatter = new ExpressionTemplate("{ {Timestamp: @t, Entry: @m, Level: @l, Exception: @x, ..@p} }\n");

            loggerConfig.WriteTo.File(
                    formatter,
                    "logs/fulfilment-web.json",
                    shared: true);

            loggerConfig.Enrich.WithAppVersion();
            if (options.Baggage.Log)
            {
                loggerConfig.Enrich.WithBaggage();
            }

            return loggerConfig.CreateLogger();
        }

    }
}
