using System;

namespace Ray2.Configuration.Creator
{
    public interface IEventPublishOptionsCreator
    {
        EventPublishOptions Create(Type type);
    }
}
