using System;

namespace Fulfilment.Processor.Configuration
{
    public class MetricsOptions
    {
        public string AppVersion {get; set; } = "1.3.1";

        public Single Factor {get; set; } = 1F;

        public Single FailureFactor {get; set; } = 0.1F;

        public bool Enabled { get; set; } = false;

        public bool IncludeRuntime { get; set; } = false;

        public int ServerPort { get; set; } = 9110;
    }
}
 