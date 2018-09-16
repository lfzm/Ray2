using Ray2.PostgreSQL.Serialization;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ray2.PostgreSQL.Configuration
{
    public class PostgreSqlOptions
    {
        public string ConnectionString { get; set; }
        public SerializationType SerializationType { get; set; } = SerializationType.JSON;
    }
    
}
