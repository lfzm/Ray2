using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Ray2.PostgreSQL.Serialization
{
    public interface ISerializer
    {
        T Deserialize<T>(byte[] bytes);
        object Deserialize(Type type, byte[] bytes);
        byte[] Serialize(object instance);
        byte[] Serialize<T>(T instance);
    }
}
