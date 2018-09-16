using System;
using System.Collections.Generic;
using System.Text;

namespace Ray2
{
    /// <summary>
    /// Event abstract class
    /// </summary>
    /// <typeparam name="TStateKey">StateId Type</typeparam>
    public abstract class Event<TStateKey> : IEvent<TStateKey>
    {
        public Event()
        {
            this.TypeCode = this.GetType().FullName;
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
        public virtual TStateKey StateId { get; set; }
        /// <summary>
        ///  the version number of <see cref="IState{TStateKey}"/>  
        /// </summary>
        public virtual long Version { get; set; }
        /// <summary>
        ///  Event release timestamp
        /// </summary>
        public virtual long Timestamp { get; } = DateTime.Now.Ticks;
        /// <summary>
        /// Event type fullname
        /// </summary>
        public virtual string TypeCode { get; }
        /// <summary>
        /// Relation Event
        /// </summary>
        public virtual string RelationEvent { get; }
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
    }
}
