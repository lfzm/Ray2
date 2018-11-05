using Ray2.Serialization;

namespace Ray2.PostgreSQL.Configuration
{
    public class PostgreSqlOptions
    {
        public string ConnectionString { get; set; }
        public SerializationType SerializationType { get; set; } = SerializationType.String;
    }

}
