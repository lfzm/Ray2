using Ray2.Storage;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ray2.Configuration
{
    public class EventSourcesConfig
    {
        public EventSourcesConfig(EventSourcingAttribute attr)
        {
            this.EventSourceName = attr.Name;
          
            this.Snapshot = new StateSnapshotConfig
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
        public string EventSourceName { get; set; }
        public StateSnapshotConfig Snapshot { get; set; }
        public MQPublishConfig MQPublish { get; set; }

      
    }
}
