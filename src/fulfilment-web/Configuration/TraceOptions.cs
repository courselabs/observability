using System;

namespace Fulfilment.Web.Configuration
{
    public class TraceOptions
    {
        public bool Console { get; set; } = true;

        public bool Jaeger { get; set; } = false;
    }
}
 