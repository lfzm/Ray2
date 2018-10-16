using System;
using System.Collections.Generic;
using System.Text;

namespace Ray2.Configuration
{
    public class SnapshotOptions: StorageOptions
    {
        public SnapshotType SnapshotType { get; set; } = SnapshotType.Asynchronous;

    }
    public enum SnapshotType
    {
        Synchronous,
        Asynchronous,
        NoSnapshot
    }
}
