using Ray2.EventSource;
using System.Collections.Generic;
using System.Linq;

namespace Ray2.Storage
{
    public class EventCollectionStorageModel 
    {
        public EventCollectionStorageModel(string eventSourceName, string storageTableName)
        {
            this.EventSourceName = eventSourceName;
            this.StorageTableName = storageTableName;
            this.Events = new List<EventModel>();
        }
        public string EventSourceName { get; }
        public string StorageTableName { get; }
        public List<EventModel> Events { get; }
        public object GetStateId()
        {
            return Events.First().Event.GetStateId();
        }
    }
}
