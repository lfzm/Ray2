using Ray2.EventSource;

namespace Ray2.Storage
{
    public class EventStorageModel : EventModel
    {
        public EventStorageModel(object stateId, IEvent @event, string eventSourceName, string storageTableName) :base(@event)
        {
            this.EventSourceName = eventSourceName;
            this.StorageTableName = storageTableName;
        }
        public string EventSourceName { get; }
        public string StorageTableName { get; }
        public bool Result { get; set; }
    }
}
