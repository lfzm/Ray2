using Ray2.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ray2.Configuration.Creator
{
    public interface IEventSubscribeOptionsCreator
    {
        IList<EventSubscribeOptions> Create(Type type);

        EventSubscribeOptions Create(EventProcessorAttribute attribute);
    }
}
