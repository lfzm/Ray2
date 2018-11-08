using Ray2.Serialization;

namespace Ray2.PostgreSQL
{
    public class PostgreSqlOptions
    {
        public string ConnectionString { get; set; }

        public string SerializationType { get; set; } = Ray2.SerializationType.JsonUTF8;
    }

}
