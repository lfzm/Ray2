using System;
using System.Collections.Generic;
using System.Text;

namespace Ray2.Serialization
{
    public interface ISerializer: IByteSerializer
    {
        T Deserialize<T>(string json);
        object Deserialize(Type type, string json);
        string SerializeString(object obj);
        string SerializeString<T>(T obj);
    }
}
