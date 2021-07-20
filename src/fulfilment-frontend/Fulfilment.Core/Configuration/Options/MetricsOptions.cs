using System;
using System.Collections.Generic;
using System.Text;

namespace Fulfilment.Core.Configuration
{
    public class MetricsOptions
    {
        public string AppVersion { get; set; } = "1.7.21";

        public bool Enabled { get; set; } = false;

        public bool IncludeRuntime { get; set; } = true;
    }
}
