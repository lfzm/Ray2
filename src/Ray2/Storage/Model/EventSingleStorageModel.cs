namespace Ray2.Storage
{
    public class EventSingleStorageModel : EventStorageModel, IEventStorageModel
    {
        public EventSingleStorageModel(object stateId, IEvent @event, string eventSourceName, string storageTableName) : base(stateId, @event)
        {
            this.EventSourceName = eventSourceName;
            this.StorageTableName = storageTableName;
        }
        public string EventSourceName { get; }
        public string StorageTableName { get; }
        public object GetStateId()
        {
            return this.StateId;
        }
    }
}
