using Ray2.Configuration.Attributes;
using Ray2.Configuration.Builder;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Ray2.Configuration.Creator
{
    public class EventProcessOptionsCreator : IEventProcessOptionsCreator
    {
        private readonly IEventSubscribeOptionsCreator _eventSubscribeOptionsCreator;
        public EventProcessOptionsCreator(IEventSubscribeOptionsCreator eventSubscribeOptionsCreator)
        {
            this._eventSubscribeOptionsCreator = eventSubscribeOptionsCreator;
        }

        public EventProcessOptions Create(Type type)
        {
            List<EventProcessOptions> options = new List<EventProcessOptions>();
            var attribute = type.GetCustomAttribute<EventProcessorAttribute>();
            if (attribute == null)
            {
                throw new Exception($"The {type.FullName} processor does not have an {nameof(EventProcessorAttribute)} configured.");
            }

            var statusOptions = this.CreateStatusOptions(attribute);
            var eventSubscribeOptionsList = this._eventSubscribeOptionsCreator.Create(type);

            return new EventProcessOptionsBuilder()
                  .WithProcessor(attribute.Name, type)
                  .WithEventSourceName(attribute.EventSourceName)
                  .WithOnceProcessConfig(attribute.OnceProcessCount, attribute.OnceProcessTimeout)
                  .WithStatusOptions(statusOptions)
                  .WithEventSubscribeOptions(eventSubscribeOptionsList)
                  .Build();
        }

        private StatusOptions CreateStatusOptions(EventProcessorAttribute attribute)
        {
            return new StatusOptions(attribute.StorageProvider, attribute.ShardingStrategy, attribute.StatusMode);
        }
       
    }
}
