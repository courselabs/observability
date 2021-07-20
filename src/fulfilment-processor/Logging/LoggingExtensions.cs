using Serilog;
using Serilog.Configuration;
using System;

namespace Fulfilment.Processor.Logging
{
    public static class LoggingExtensions
    {
        public static LoggerConfiguration WithAppVersion(this LoggerEnrichmentConfiguration enrich)
        {
            if (enrich == null)
                throw new ArgumentNullException(nameof(enrich));

            return enrich.With<AppVersionEnricher>();
        }

        public static LoggerConfiguration WithAppName(this LoggerEnrichmentConfiguration enrich)
        {
            if (enrich == null)
                throw new ArgumentNullException(nameof(enrich));

            return enrich.With<AppNameEnricher>();
        }
    }
}
