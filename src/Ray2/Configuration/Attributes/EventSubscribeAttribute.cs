using Ray2.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ray2
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class EventSubscribeAttribute : Attribute
    {
        public EventSubscribeAttribute(string name, string MQProvider)
            : this(name, MQProvider, null, null)
        {

        }
        public EventSubscribeAttribute(string name, string MQProvider, string eventSourceName, string snapshotName)
        {
            this.Name = name;
            this.EventSourceName = eventSourceName;
            this.MQProvider = MQProvider;
            this.SnapshotName = snapshotName;
        }
        public string Name { get; set; }
        public string EventSourceName { get; set; }
        public string MQProvider { get; set; }
        public string SnapshotName { get; set; }
        public int OnceProcessCount { get; set; } = 1;
        public TimeSpan OnceProcessTimeout { get; set; } = TimeSpan.FromMinutes(5);
        public SnapshotType SnapshotType { get; set; } = SnapshotType.Asynchronous;
    }
}
