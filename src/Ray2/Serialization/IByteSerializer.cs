using System;

namespace Ray2.Serialization
{
    public interface IByteSerializer
    {
        T Deserialize<T>(byte[] bytes);
        object Deserialize(Type type, byte[] bytes);
        byte[] Serialize(object obj);
        byte[] Serialize<T>(T obj);
    }
}
