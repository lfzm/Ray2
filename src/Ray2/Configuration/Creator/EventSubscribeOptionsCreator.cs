using Ray2.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ray2.Configuration.Creator
{
    public class EventSubscribeOptionsCreator : IEventSubscribeOptionsCreator
    {
        public IList<EventSubscribeOptions> Create(Type type)
        {
            throw new NotImplementedException();
        }

        public EventSubscribeOptions Create(EventProcessorAttribute attribute)
        {
            throw new NotImplementedException();
        }
    }
}
