using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ray2.EventSource
{
    /// <summary>
    ///  Transaction batch event
    /// </summary>
    /// <typeparam name="TState"><see cref="IState{TStateKey}"/></typeparam>
    /// <typeparam name="TStateKey"></typeparam>
    public interface IEventTransaction<TState, TStateKey>: IEventTransaction
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
        /// Write events to a transaction
        /// </summary>
        /// <param name="events">event list</param>
        /// <param name="isPublish">Whether to publish to mq</param>
        void WriteEventAsync(IList<IEvent<TStateKey>> events, bool isPublish = true);
    }
    /// <summary>
    ///  Transaction batch event
    /// </summary>
    public interface IEventTransaction
    {
        TransactionState TransactionState { get; }
        /// <summary>
        /// Transaction volume
        /// </summary>
        /// <returns></returns>
        int Count();
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
