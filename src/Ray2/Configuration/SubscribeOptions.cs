using System;
using System.Collections.Generic;
using System.Text;

namespace Ray2.Configuration
{
    public class SubscribeOptions
    {
        public string ProviderName { get; set; }
        public string Topic { get; set; }
        public string Group { get; set; }
    }
}
