using System;
using System.Collections.Generic;
using System.Text;

namespace Ray2.Serialization
{
   public interface IByteSerializer
    {
        T Deserialize<T>(byte[] bytes);
        object Deserialize(Type type, byte[] bytes);
        byte[] Serialize(object instance);
        byte[] Serialize<T>(T instance);
    }
}
