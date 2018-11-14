using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Ray2
{
    /// <summary>
    /// Event abstract class
    /// </summary>
    /// <typeparam name="TStateKey">StateId Type</typeparam>
    [DataContract]
    public abstract class Event<TStateKey> : IEvent<TStateKey>
    {
        public Event()
        {
            this.TypeCode = this.GetType().FullName;
            this.Timestamp = this.GetCurrentTimeUnix();
        }
        /// <summary>
        /// Event abstract class
        /// </summary>
        /// <param name="event">Relation Event</param>
        public Event(IEvent @event) : this()
        {
            this.RelationEvent = @event.GetRelationKey();
        }
        /// <summary>
        /// State Id  <see cref="IState{TStateKey}"/>  
        /// </summary>
        [DataMember(Order = 1)]
        public virtual TStateKey StateId { get; set; }
        /// <summary>
        ///  the version number of <see cref="IState{TStateKey}"/>  
        /// </summary>
        [DataMember(Order = 2)]
        public virtual long Version { get; set; }
        /// <summary>
        ///  Event release timestamp
        /// </summary>
        [DataMember(Order = 3)]
        public virtual long Timestamp { get; private set; }
        /// <summary>
        /// Event type fullname
        /// </summary>
        [DataMember(Order = 4)]
        public virtual string TypeCode { get; private set; }
        /// <summary>
        /// Relation Event
        /// </summary>
        [DataMember(Order = 5)]
        public virtual string RelationEvent { get;  set; }
        /// <summary>
        ///  Generate Relation key
        /// </summary>
        /// <returns></returns>
        public string GetRelationKey()
        {
            return $"{StateId}-{TypeCode}-{Version}";
        }
        /// <summary>
        /// Get StateId
        /// </summary>
        /// <returns></returns>
        public object GetStateId()
        {
            return this.StateId;
        }
        public override string ToString()
        {
            return $"TypeCode:{TypeCode},StateId:{StateId},VersionNo:{Version},Timestamp:{Timestamp}";
        }


        private long GetCurrentTimeUnix()
        {
            TimeSpan cha = (DateTime.Now - TimeZoneInfo.ConvertTimeToUtc(new System.DateTime(1970, 1, 1)));
            long t = (long)cha.TotalMilliseconds;
            return t;
        }

    }
}
