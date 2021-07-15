namespace Fulfilment.Authorization.Configuration
{
    public class TraceOptions
    {
        public bool Console { get; set; } = true;

        public bool Jaeger { get; set; } = false;

        public AgentOptions Agent { get; set; } = new AgentOptions();
    }
}
 