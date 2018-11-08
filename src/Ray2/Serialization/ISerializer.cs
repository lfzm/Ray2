using System;
using System.Collections.Generic;
using System.Text;

namespace Ray2.Serialization
{
    public interface ISerializer    {
        T Deserialize<T>(byte[] bytes);
        object Deserialize(Type type, byte[] bytes);
        byte[] Serialize(object obj);
        byte[] Serialize<T>(T obj);
    }
}
