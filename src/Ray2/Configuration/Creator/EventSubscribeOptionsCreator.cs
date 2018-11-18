using Ray2.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Ray2.Configuration.Creator
{
    public class EventSubscribeOptionsCreator : IEventSubscribeOptionsCreator
    {
        public IList<EventSubscribeOptions> Create(Type type)
        {
            IList<EventSubscribeOptions> optionsList = new List<EventSubscribeOptions>();
            var attribute = type.GetCustomAttribute<EventProcessorAttribute>();
            var options = this.CreateEventSubscribeOptions(type.FullName, attribute);
            optionsList.Add(options);

            var attributes = type.GetCustomAttributes<EventSubscribeAttribute>();
            if (attributes == null || attributes.Count() == 0)
                return optionsList;

            foreach (var attr in attributes)
            {
                options = this.CreateEventSubscribeOptions(type.FullName, attr);
                optionsList.Add(options);
            }
            return optionsList;
        }


        private EventSubscribeOptions CreateEventSubscribeOptions(string fullName, EventSubscribeAttribute attribute)
        {
            return new EventSubscribeOptions(attribute.MQProvider, attribute.Topic, fullName);
        }
        private EventSubscribeOptions CreateEventSubscribeOptions(string fullName, EventProcessorAttribute attribute)
        {
            return new EventSubscribeOptions(attribute.MQProvider, attribute.Topic, fullName);
        }
    }
}
