using System;
using System.Collections.Generic;
using System.Text;

namespace Ray2.Configuration
{
    public class StatusOptions : StorageOptions
    {
        public StatusOptions(string storageProvider, string shardingStrategy, StatusMode statusMode) : base(storageProvider, shardingStrategy)
        {
            this.StatusMode = statusMode;
        }
        public StatusMode StatusMode { get; private set; }
    }

    public enum StatusMode
    {
        Synchronous,
        Asynchronous
    }
}
