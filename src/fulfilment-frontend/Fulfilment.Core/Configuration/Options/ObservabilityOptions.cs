namespace Fulfilment.Core.Configuration
{
    public class ObservabilityOptions
    {  
        public TraceOptions Trace { get; set; } = new TraceOptions();

        public MetricsOptions Metrics { get; set; } = new MetricsOptions();

        public LoggingOptions Logging { get; set; } = new LoggingOptions();
    }
}
