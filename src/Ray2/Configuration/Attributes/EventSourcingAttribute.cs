using Ray2.Configuration;
using System;

namespace Ray2
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class EventSourcingAttribute : Attribute
    {
        public EventSourcingAttribute(string storageProviderName, string mqProviderName)
        {
            this.MQProvider = mqProviderName;
            this.StorageProvider = storageProviderName;
        }
        public string Name { get; set; }
        public string MQProvider { get; set; }
        public string Topic { get; set; }
        public string StorageProvider { get; set; }
        public string ShardingStrategy { get; set; }
        public SnapshotType SnapshotType { get; set; } = SnapshotType.Asynchronous;
    }
}
