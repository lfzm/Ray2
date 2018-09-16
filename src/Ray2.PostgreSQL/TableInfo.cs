using System;
using System.Collections.Generic;
using System.Text;

namespace Ray2.PostgreSQL
{
    public class TableInfo
    {
        public string Name { get; set; }
        public string Prefix { get; set; }
        public long Version { get; set; }
        public long CreateTime { get; set; }
        public long NextVersion()
        {
            return Version += 1;
        }
    }


}
