using Serilog;
using Serilog.Configuration;
using System;

namespace Fulfilment.Core.Logging
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

        public static LoggerConfiguration WithBaggage(this LoggerEnrichmentConfiguration enrich)
        {
            if (enrich == null)
                throw new ArgumentNullException(nameof(enrich));

            return enrich.With<BaggageEnricher>();
        }
    }
}
