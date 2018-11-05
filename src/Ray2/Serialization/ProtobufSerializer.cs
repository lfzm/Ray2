using System;
using System.Text;

namespace Ray2.Serialization
{
    public class ProtobufSerializer : IByteSerializer
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
                return ms.ToArray();
            }
        }
        public byte[] Serialize<T>(T instance)
        {
           return  this.Serialize(instance);
        }
    }
}
