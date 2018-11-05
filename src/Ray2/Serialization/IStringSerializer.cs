using System;

namespace Ray2.Serialization
{
    public interface IStringSerializer
    {
        T Deserialize<T>(string json);
        object Deserialize(Type type, string json);
        string Serialize(object obj);
        string Serialize<T>(T obj);
    }
}
