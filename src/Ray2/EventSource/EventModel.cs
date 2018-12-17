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
            StateId = @event.GetStateId();
        }
        public EventModel(IEvent @event):this(@event, @event.TypeCode, @event.Version)
        {
           
        }
        public IEvent Event { get; }
        public string TypeCode { get; }
        public Int64 Version { get; }
        public object StateId { get; }
    }
}
