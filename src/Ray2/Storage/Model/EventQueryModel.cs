using System;
using System.Collections.Generic;
using System.Text;

namespace Ray2.Storage
{
    public class EventQueryModel
    {
        public EventQueryModel(Int64 startVersion, Int64? startTime = null)
        {
            this.StartVersion = startVersion;
            this.StartTime = startTime;
        }
        public EventQueryModel(Int64 startVersion,Int64 endVersion, Int64? startTime = null):this( startVersion, startTime)
        {
            this.EndVersion = endVersion;
        }
        public object StateId { get;internal set; }
        public Int64 StartVersion { get; set; }
        public Int64 EndVersion { get; set; }
        public string EventTypeCode { get; set; }
        public string RelationEvent { get; set; }
        public Int64? StartTime { get; set; }
        public Int32 Limit { get; set; }
    }
}
