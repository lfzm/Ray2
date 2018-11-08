using Ray2.Serialization;
using System;
using System.Text;

namespace Ray2.Protobuf
{
    public class ProtobufSerializer : ISerializer
    {
        Encoding encoding = Encoding.UTF8;
        public T Deserialize<T>(byte[] bytes)
        {
            using (PooledMemoryStream ms = new PooledMemoryStream(bytes))
            {
                return ProtoBuf.Serializer.Deserialize<T>(ms);
            }
        }

        public object Deserialize(Type type, byte[] bytes)
        {
            using (PooledMemoryStream ms = new PooledMemoryStream(bytes))
            {
                return ProtoBuf.Serializer.Deserialize(type, ms);
            }
        }

        public byte[] Serialize(object instance)
        {
            using (PooledMemoryStream ms = new PooledMemoryStream())
            {
                ProtoBuf.Serializer.Serialize(ms, instance);
                var data = ms.ToArray();
                if (data.Length == 0)
                    throw new Exception("Protobuf serialization failed, data is empty after serialization");
                else
                    return data;
            }
        }
        public byte[] Serialize<T>(T instance)
        {
            using (PooledMemoryStream ms = new PooledMemoryStream())
            {
                ProtoBuf.Serializer.Serialize(ms, instance);
                var data = ms.ToArray();
                if (data.Length == 0)
                    throw new Exception("Protobuf serialization failed, data is empty after serialization");
                else
                    return data;
            }
        }
    }
}
