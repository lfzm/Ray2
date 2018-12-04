using System;

namespace Ray2
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class EventPublishAttribute : Attribute
    {
        public EventPublishAttribute(string topic)
        {
            this.Topic = topic;
        }
        /// <summary>
        /// This is a message queue provider
        /// </summary>
        public string MQProvider { get; set; }
        /// <summary>
        /// mq topic
        /// </summary>
        public string Topic { get; set; }
    }
}
