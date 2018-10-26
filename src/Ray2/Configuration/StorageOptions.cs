using System;
using System.Collections.Generic;
using System.Text;

namespace Ray2.Configuration
{
    public class StorageOptions
    {
        public StorageOptions(string storageProvider,string shardingStrategy)
        {
            this.StorageProvider = storageProvider;
            this.ShardingStrategy = shardingStrategy;
        }
        public string StorageProvider { get; private set; }
        public string ShardingStrategy { get; private set; }
    }
}
