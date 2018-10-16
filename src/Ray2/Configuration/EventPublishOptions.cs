using System;
using System.Collections.Generic;
using System.Text;

namespace Ray2.Configuration
{
    public class EventPublishOptions
    {
        public string MQProvider{ get; set; }
        public string Topic { get; set; }
    }
}
