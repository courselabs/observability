namespace Fulfilment.Processor.Configuration
{
    public class LoggingOptions
    {
        public bool Console { get; set; } = true;

        public bool File { get; set; } = false;

        public bool Structured { get; set; } = true;
    }
}

