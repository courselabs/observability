namespace Fulfilment.Processor.Configuration
{
    public class ObservabilityOptions
    {        
        public int StartupDelaySeconds {get; set; } = 0;

        public int ExitAfterSeconds {get; set; } = 0;

        public LoggingOptions Logging { get; set; } = new LoggingOptions();

        public MetricsOptions Metrics { get; set; } = new MetricsOptions();
    }
}
