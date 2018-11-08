using System.Collections.Generic;
using System.Linq;

namespace Ray2.Storage
{
    public class EventCollectionStorageModel : IEventStorageModel
    {
        public EventCollectionStorageModel(string eventSourceName, string storageTableName)
        {
            this.EventSourceName = eventSourceName;
            this.StorageTableName = storageTableName;
            this.Events = new List<EventStorageModel>();
        }
        public string EventSourceName { get; }
        public string StorageTableName { get; }
        public List<EventStorageModel> Events { get; }

        public object GetStateId()
        {
            return Events.First().StateId;
        }
    }
}
