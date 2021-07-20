using Fulfilment.Core.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace Fulfilment.Core.Logging
{
    public class SetupLogger
    {
        private static bool _WrittenStandard = false;
        private static readonly List<string> _Written = new List<string>();

        private readonly ILogger _logger;
        private readonly ObservabilityOptions _options;

        public SetupLogger(ObservabilityOptions options, ILogger<SetupLogger> logger)
        {
            _logger = logger;
            _options = options;
        }

        public void WriteStandard()
        {
            if (!_WrittenStandard)
            {
                _logger.LogInformation("Logging to file path: {LogFilePath}", _options.Logging.LogFilePath);
                _logger.LogInformation("Metrics enabled: {MetricsEnabled}; including runtime metrics: {MetricsRuntimeEnabled}", _options.Metrics.Enabled, _options.Metrics.IncludeRuntime);
                _logger.LogInformation("Trace exporting to console: {ConsoleTraceEnabled}; to Jaeger: {JaegerTraceEnabled}", _options.Trace.Console, _options.Trace.Jaeger);
                _WrittenStandard = true;
            }
        }

        public void LogInformation(string key, string message, params object[] args)
        {
            WriteStandard();
            if (!_Written.Contains(key))
            {
                _logger.LogInformation(message, args);
                _Written.Add(key);
            }
        }

        public void LogWarning(string key, string message, params object[] args)
        {
            WriteStandard();
            if (!_Written.Contains(key))
            {
                _logger.LogWarning(message, args);
                _Written.Add(key);
            }
        }
    }
}
