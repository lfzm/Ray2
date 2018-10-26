using Ray2.Storage;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ray2.Configuration
{
    public class EventSourceOptions
    {
        public EventSourceOptions(string eventSourceName,string sourcingFullName, SnapshotOptions snapshotOptions , StorageOptions storageOptions)
        {
            this.EventSourceName = eventSourceName;
            this.SourcingFullName = sourcingFullName;
            this.SnapshotOptions = snapshotOptions;
            this.StorageOptions = storageOptions;
        }
        public string EventSourceName { get; private set; }
        public string SourcingFullName { get; private set; }
        public SnapshotOptions SnapshotOptions { get; private set; }
        public StorageOptions StorageOptions { get; private set; }
    }
}
