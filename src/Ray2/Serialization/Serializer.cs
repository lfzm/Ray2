using System;
using System.Collections.Generic;
using System.Text;

namespace Ray2.Serialization
{
    public class Serializer : ISerializer
    {
        private readonly IByteSerializer _byteSerializer;
        private readonly IStringSerializer _stringSerializer;
        public Serializer(IByteSerializer byteSerializer, IStringSerializer stringSerializer)
        {
            this._byteSerializer = byteSerializer;
            this._stringSerializer = stringSerializer;
        }
        public T Deserialize<T>(string json)
        {
           return this._stringSerializer.Deserialize<T>(json);
        }

        public object Deserialize(Type type, string json)
        {
            return this._stringSerializer.Deserialize(type,json);
        }

        public T Deserialize<T>(byte[] bytes)
        {
            return this._byteSerializer.Deserialize<T>(bytes);
        }

        public object Deserialize(Type type, byte[] bytes)
        {
            return this._byteSerializer.Deserialize(type, bytes);
        }

        public byte[] Serialize(object obj)
        {
            return this._byteSerializer.Serialize(obj);
        }

        public byte[] Serialize<T>(T obj)
        {
            return this._byteSerializer.Serialize<T>(obj);
        }

        public string SerializeString(object obj)
        {
            return this._stringSerializer.Serialize(obj);
        }

        public string SerializeString<T>(T obj)
        {
            return this._stringSerializer.Serialize<T>(obj);
        }
    }
}
