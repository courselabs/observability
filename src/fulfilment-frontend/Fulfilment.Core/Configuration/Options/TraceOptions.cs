using System;

namespace Fulfilment.Core.Configuration
{
    public class TraceOptions
    {
        public bool Console { get; set; } = true;

        public bool Jaeger { get; set; } = false;

        public string HeaderFormat { get; set; } = "W3C";

        public AgentOptions Agent { get; set; } = new AgentOptions();

        public bool CustomSpans { get; set; } = false;

        public BaggageOptions Baggage { get; set; } = new BaggageOptions();
    }
}
 