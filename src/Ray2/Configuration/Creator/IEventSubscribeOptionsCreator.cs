using System;
using System.Collections.Generic;

namespace Ray2.Configuration.Creator
{
    public interface IEventSubscribeOptionsCreator
    {
        IList<EventSubscribeOptions> Create(Type type);

    }
}
