using System;

namespace Ray2.EventSource
{
    public class EventModel
    {
        public EventModel(IEvent @event, string typeCode, long version)
        {
            Event = @event;
            TypeCode = typeCode;
            Version = version;
        }
        public EventModel(IEvent @event)
        {
            Event = @event;
            TypeCode = @event.TypeCode;
            Version = @event.Version;
        }
        public IEvent Event { get; }
        public string TypeCode { get; }
        public Int64 Version { get; }
    }
}
