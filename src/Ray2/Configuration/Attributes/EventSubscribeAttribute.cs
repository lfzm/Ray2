using Ray2.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ray2
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class EventSubscribeAttribute : Attribute
    {
        public EventSubscribeAttribute(string topic, string mqprovider)
        {
            this.Topic = topic;
            this.MQProvider = mqprovider;
        }
        public string Topic { get; set; }
        public string MQProvider { get; set; }
    }

}
