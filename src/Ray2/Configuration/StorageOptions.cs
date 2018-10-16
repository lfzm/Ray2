using System;
using System.Collections.Generic;
using System.Text;

namespace Ray2.Configuration
{
    public class StorageOptions
    {
        public string StorageProvider { get; set; }
        public string ShardingStrategy { get; set; }
    }
}
