using Ray2.Configuration;
using Ray2.Storage;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ray2
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class EventSourcingAttribute : Attribute
    {
        public string Name { get; set; }
        public string StorageProvider { get; set; }
        public string MQProvider { get; set; }
        public bool IsTableSharding { get; set; }
        public int ShardingTableCount { get; set; }
        public string SnapshotName { get; set; }
        public SnapshotType SnapshotType { get; set; } = SnapshotType.Asynchronous;
    }
}
