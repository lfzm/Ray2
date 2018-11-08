namespace Ray2.Storage
{
    public class EventStorageModel
    {
        public EventStorageModel(object stateId, IEvent @event)
        {
            this.StateId = stateId;
            this.Event = @event;
        }
        public IEvent Event { get; }
        public object StateId { get; }
    }
}
