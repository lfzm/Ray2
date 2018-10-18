using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Ray2.EventSource
{
    /// <summary>
    ///  Transaction batch event
    /// </summary>
    /// <typeparam name="TState"><see cref="IState{TStateKey}"/></typeparam>
    /// <typeparam name="TStateKey"></typeparam>
    public interface IEventTransaction<TState, TStateKey>
    {
        /// <summary>
        /// Transaction copy status
        /// </summary>
        TState State { get; }
        /// <summary>
        /// Write events to a transaction
        /// </summary>
        /// <param name="event">event</param>
        /// <param name="isPublish">Whether to publish to mq</param>
        void WriteEventAsync(IEvent<TStateKey> @event, bool isPublish = true);
        /// <summary>
        /// Commit transaction
        /// </summary>
        /// <returns></returns>
        Task<bool> Commit();
        /// <summary>
        /// roll back transaction
        /// </summary>
        void Rollback();
    }
}
