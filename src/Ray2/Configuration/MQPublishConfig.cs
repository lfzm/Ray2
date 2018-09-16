using System;
using System.Collections.Generic;
using System.Text;

namespace Ray2.Configuration
{
    public class MQPublishConfig
    {
        public string MQProvider{ get; set; }
        public string Topic { get; set; }
    }
}
