//using ProtoBuf;

using System.Runtime.Serialization;

namespace Ray2.MQ
{
    [DataContract]
    public class EventPublishMessage
    {
        [DataMember(Order = 1)]
        public string TypeCode { get; set; }
        [DataMember(Order = 2)]
        public object Event { get; set; }
    }
}
