//using ProtoBuf;

using System;
using System.Runtime.Serialization;

namespace Ray2.RabbitMQ
{
    [DataContract]
    public class EventPublishMessage
    {
        [DataMember(Order = 1)]
        public string TypeCode { get; set; }
        [DataMember(Order = 2)]
        public Int64 Version { get; set; }
        [DataMember(Order = 3)]
        public byte[] Event { get; set; }
    }
}
