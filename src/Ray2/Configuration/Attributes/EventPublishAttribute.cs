using System;

namespace Ray2
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class EventPublishAttribute : Attribute
    {
        public string MQProvider { get; set; }
        public string Topic { get; set; }
    }
}
