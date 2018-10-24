using System;
using System.Collections.Generic;
using System.Text;

namespace Ray2
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class EventPublishAttribute : Attribute
    {
        public string MQProvider { get; set; }
        public string MQTopic { get; set; }
    }
}
