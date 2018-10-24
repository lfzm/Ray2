using System;
using System.Text;
using ProtoBuf;

namespace Ray2.PostgreSQL.Serialization
{
    public class ProtobufSerializer : ISerializer
    {
        Encoding encoding = Encoding.UTF8;
        public T Copy<T>(T instance)
        {
            var bytes = this.Serialize(instance);
            return this.Deserialize<T>(bytes);
        }

        public object Deserialize(Type type, byte[] bytes)
        {
            using (PooledMemoryStream ms = new PooledMemoryStream(bytes))
            {
                return Serializer.Deserialize(type, ms);
            }
        }
        public T Deserialize<T>(byte[] bytes)
        {
            using (PooledMemoryStream ms = new PooledMemoryStream(bytes))
            {
                return Serializer.Deserialize<T>(ms);
            }
        }
        public byte[] Serialize<T>(T instance)
        {
            using (PooledMemoryStream ms = new PooledMemoryStream())
            {
                Serializer.Serialize(ms, instance);
                return ms.ToArray();
            }
        }

        public byte[] Serialize(object instance)
        {
            
        }
    }
}
