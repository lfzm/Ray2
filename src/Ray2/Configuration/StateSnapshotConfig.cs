using System;
using System.Collections.Generic;
using System.Text;

namespace Ray2.Configuration
{
    public class StateSnapshotConfig
    {
        public StateSnapshotConfig()
        {
            this.SnapshotType = SnapshotType.NoSnapshot;
        }
        public string SnapshotName { get; set; }
        public SnapshotType SnapshotType { get; set; }

    }
    public enum SnapshotType
    {
        Synchronous,
        Asynchronous,
        NoSnapshot
    }
}
