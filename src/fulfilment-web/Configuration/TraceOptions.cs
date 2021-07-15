using System;

namespace Fulfilment.Web.Configuration
{
    public class TraceOptions
    {
        public bool Console { get; set; } = true;

        public bool Jaeger { get; set; } = false;

        public AgentOptions Agent { get; set; } = new AgentOptions();

        public bool CustomSpans { get; set; } = false;
    }
}
 