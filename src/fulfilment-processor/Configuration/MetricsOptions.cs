namespace Fulfilment.Processor.Configuration
{
    public class MetricsOptions
    {
        public bool Enabled { get; set; } = false;

        public bool IncludeRuntime { get; set; } = false;

        public int ServerPort { get; set; } = 9110;
    }
}
