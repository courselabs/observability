namespace Fulfilment.Processor.Configuration
{
    public class LoggingOptions
    {
        public bool Console { get; set; } = true;

        public bool File { get; set; } = false;

        public bool Structured { get; set; } = true;

        public bool UseInfoForDurations { get; set; } = false;

        public string LogFilePath { get; set; } = "logs/fulfilment-processor.json";
    }
}

