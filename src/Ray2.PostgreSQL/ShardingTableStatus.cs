using System;
using System.Collections.Generic;
using System.Text;

namespace Ray2.PostgreSQL
{
    internal class ShardingTableStatus
    {
        public string Name { get; set; }
        public long Version { get; set; }
        public int Count { get; set; }
        public DateTime LastQueryTime { get; set; }
    }
}
