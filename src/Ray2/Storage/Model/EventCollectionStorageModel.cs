using Ray2.Configuration;
using System.Collections.Generic;

namespace Ray2.Storage
{
    public class EventCollectionStorageModel: IEventStorageModel
    {
        public EventCollectionStorageModel(string eventSourceName, string storageTableName)
        {
            this.EventSourceName = eventSourceName;
            this.StorageTableName = storageTableName;
            this.Events = new List<EventStorageModel>();
        }
        public string EventSourceName { get; }
        public List<EventStorageModel> Events { get; }
        public string StorageTableName { get; }
        public int Count()
        {
            return this.Events.Count;
        }
    }
}
