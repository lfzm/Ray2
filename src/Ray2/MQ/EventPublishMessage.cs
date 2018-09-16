//using ProtoBuf;

namespace Ray2.MQ
{
    //[ProtoContract(ImplicitFields = ImplicitFields.AllFields)]
    public class EventPublishMessage
    {
        public string TypeCode { get; set; }
        public object Event { get; set; }
    }
}
