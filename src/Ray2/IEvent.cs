using System;
using System.Collections.Generic;
using System.Text;

namespace Ray2
{
    /// <summary>
    /// This is the event data interface 
    /// </summary>
    public interface IEvent
    {
        /// <summary>
        ///  the version number of <see cref="IState"/>  
        /// </summary>
        Int64 Version { get; set; }
        /// <summary>
        ///  Event release timestamp
        /// </summary>
        long Timestamp { get; }
        /// <summary>
        /// Event type fullname
        /// </summary>
        string TypeCode { get; }
        /// <summary>
        /// Related event
        /// </summary>
        string RelationEvent { get; }
        /// <summary>
        /// Generate Relation key
        /// </summary>
        /// <returns></returns>
        string GetRelationKey();
        /// <summary>
        /// Get StateId
        /// </summary>
        /// <returns></returns>
        object GetStateId();
    }
    /// <summary>
    /// This is the event data interface 
    /// </summary>
    public interface IEvent<TPrimaryKey>: IEvent
    {
        /// <summary>
        /// State Id
        /// </summary>
        TPrimaryKey StateId { get; set; }
    }
}
