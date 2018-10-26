using System;
using System.Collections.Generic;
using System.Text;

namespace Ray2.Configuration
{
    public class EventSubscribeOptions
    {
        public EventSubscribeOptions(string mqProvider, string topic,string group)
        {
            this.MQProvider = mqProvider;
            this.Topic = topic;
            this.Group = group;
        }
        public string MQProvider { get; private set; }
        public string Topic { get; private set; }
        public string Group { get; private set; }
    }
}
