namespace Fulfilment.Processor.Configuration
{
    public class ObservabilityOptions
    {
        public LoggingOptions Logging { get; set; } = new LoggingOptions();

        public MetricsOptions Metrics { get; set; } = new MetricsOptions();
    }
}
