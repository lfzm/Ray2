using Ray2.Configuration;
using Ray2.EventProcess;
using Ray2.EventSources;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ray2.Configuration
{
    public class EventProcessConfig
    {
        public EventProcessConfig(EventSubscribeAttribute attr, Type type)
        {
            this.ProcessType = type;
            this.ProcessorName = attr.Name;
            this.EventSourceName = attr.EventSourceName;
            this.OnceProcessCount = attr.OnceProcessCount;
            this.OnceProcessTimeout = attr.OnceProcessTimeout;
            this.Snapshot = new StateSnapshotConfig()
            {
                SnapshotName = attr.SnapshotName,
                SnapshotType = attr.SnapshotType
            };
            if (!string.IsNullOrEmpty(attr.MQProvider))
            {
                this.MQPublish = new MQPublishConfig
                {
                    MQProvider = attr.MQProvider,
                    Topic = this.EventSourceName
                };
            }
        }
        public string ProcessorName { get; set; }
        public string EventSourceName { get; set; }
        public int OnceProcessCount { get; set; }
        public TimeSpan OnceProcessTimeout { get; set; }
        public Type ProcessType { get; set; }
        public StateSnapshotConfig Snapshot { get; set; }
        public MQPublishConfig MQPublish { get; set; }

    }
}
