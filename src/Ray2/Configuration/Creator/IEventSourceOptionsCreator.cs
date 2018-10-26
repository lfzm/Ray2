using System;

namespace Ray2.Configuration.Creator
{
    public interface IEventSourceOptionsCreator
    {
        EventSourceOptions Create(Type type);
    }
}
