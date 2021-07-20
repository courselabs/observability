using Serilog.Core;
using Serilog.Events;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Fulfilment.Processor.Logging
{
    public class AppNameEnricher : ILogEventEnricher
    {
        LogEventProperty _cachedProperty;

        public const string PropertyName = "AppName";

        /// <summary>
        /// Enrich the log event.
        /// </summary>
        /// <param name="logEvent">The log event to enrich.</param>
        /// <param name="propertyFactory">Factory for creating new properties to add to the event.</param>
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            logEvent.AddPropertyIfAbsent(GetLogEventProperty(propertyFactory));
        }

        private LogEventProperty GetLogEventProperty(ILogEventPropertyFactory propertyFactory)
        {
            if (_cachedProperty == null)
            {
                _cachedProperty = CreateProperty(propertyFactory);
            }
            return _cachedProperty;
        }

        // Qualify as uncommon-path
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static LogEventProperty CreateProperty(ILogEventPropertyFactory propertyFactory)
        {
            var value = Assembly.GetEntryAssembly().GetName().Name;
            return propertyFactory.CreateProperty(PropertyName, value);
        }
    }
}
