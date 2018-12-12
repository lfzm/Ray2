using System;
using System.Reflection;

namespace Ray2.Configuration.Creator
{
    public class EventPublishOptionsCreator : IEventPublishOptionsCreator
    {
        public EventPublishOptions Create(Type type)
        {
            var attr = type.GetCustomAttribute<EventSourcingAttribute>();
            if (attr != null)
            {
                string topic = attr.Topic;
                if (string.IsNullOrEmpty(topic))
                {
                    topic = type.Name;
                }
                return new EventPublishOptions(topic, attr.MQProvider, type.FullName);
            }

            var attribute = type.GetCustomAttribute<EventPublishAttribute>();
            if (attribute == null)
            {
                return null;
            }
            return new EventPublishOptions(attribute.Topic, attribute.MQProvider, type.FullName);
        }
    }
}
