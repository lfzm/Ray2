//using ProtoBuf;

using Ray2.EventSource;
using Ray2.Serialization;
using System;
using System.Runtime.Serialization;

namespace Ray2.RabbitMQ
{
    [DataContract]
    public class PublishMessage
    {
        public PublishMessage() { }
        public PublishMessage(EventModel model, ISerializer serializer)
        {
            this.TypeCode = model.TypeCode;
            this.Version = model.Version;
            this.Data = serializer.Serialize(model.Event);
        }
        [DataMember(Order = 1)]
        public string TypeCode { get; set; }
        [DataMember(Order = 2)]
        public Int64 Version { get; set; }
        [DataMember(Order = 3)]
        public byte[] Data { get; set; }
    }
}
