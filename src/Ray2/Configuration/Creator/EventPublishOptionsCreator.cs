using System;
using System.Reflection;

namespace Ray2.Configuration.Creator
{
    public class EventPublishOptionsCreator : IEventPublishOptionsCreator
    {
        public EventPublishOptions Create(Type type)
        {
            var attribute = type.GetCustomAttribute<EventPublishAttribute>();
            if (attribute == null)
            {
                return null;
            }
            return new EventPublishOptions(attribute.Topic, attribute.MQProvider, type.FullName);
        }
    }
}
