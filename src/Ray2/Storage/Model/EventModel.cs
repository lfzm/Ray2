using System;
using System.Collections.Generic;
using System.Text;

namespace Ray2.Storage
{
   public class EventModel
    {
        public EventModel(object data, string typeCode, long version)
        {
            Data = data;
            TypeCode = typeCode;
            Version = version;
        }
        public object Data { get; }
        public string TypeCode { get; }
        public Int64 Version { get; }
    }
}
