using System;
using System.Collections.Generic;
using System.Text;

namespace Ray2.Configuration
{
    public class SnapshotOptions : StorageOptions
    {
        public SnapshotOptions(string storageProvider, string shardingStrategy, SnapshotType snapshotType) 
            : base(storageProvider, shardingStrategy)
        {
            this.SnapshotType = snapshotType;
        }

        public SnapshotOptions() 
            : base(null, null)
        {
            this.SnapshotType = SnapshotType.NoSnapshot;
        }
        public SnapshotType SnapshotType { get; private set; }

    }
    public enum SnapshotType
    {
        Synchronous,
        Asynchronous,
        NoSnapshot
    }
}
