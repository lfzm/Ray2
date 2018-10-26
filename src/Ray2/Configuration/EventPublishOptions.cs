using System;
using System.Collections.Generic;
using System.Text;

namespace Ray2.Configuration
{
    public class EventPublishOptions
    {
        public EventPublishOptions(string topic, string mqProvider)
        {
            this.Topic = topic;
            this.MQProvider = mqProvider;
        }
        public string Topic { get; private set; }
        public string MQProvider { get; private set; }
    }
}
