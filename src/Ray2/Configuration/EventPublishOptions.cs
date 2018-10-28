using System;
using System.Collections.Generic;
using System.Text;

namespace Ray2.Configuration
{
    public class EventPublishOptions
    {
        public EventPublishOptions(string topic, string mqProvider,string fullName)
        {
            this.Topic = topic;
            this.MQProvider = mqProvider;
            this.EventPublishFullName = fullName;
        }
        public string EventPublishFullName { get; private set; }
        public string Topic { get; private set; }
        public string MQProvider { get; private set; }
    }
}
