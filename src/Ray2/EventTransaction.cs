using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;

namespace Ray2
{
    /// <summary>
    /// Transaction batch event
    /// </summary>
    /// <typeparam name="TState"><see cref="IState{TStateKey}"/></typeparam>
    /// <typeparam name="TStateKey"></typeparam>
    public class EventTransaction<TState, TStateKey> : IEventTransaction<TState, TStateKey>
            where TState : IState<TStateKey>, new()
    {
        private readonly RayGrain<TState, TStateKey> rayGrain;
        private readonly IEventSourcing<TState, TStateKey> eventSourcing;

        private IList<IEvent<TStateKey>> transactionEvents = new List<IEvent<TStateKey>>();
        private IList<IEvent> publishEvents = new List<IEvent>();
        private TransactionState transactionState =  TransactionState.NotCommit;
        public EventTransaction(RayGrain<TState, TStateKey> rayGrain, IEventSourcing<TState, TStateKey> eventSourcing)
        {
            this.rayGrain = rayGrain;
            this.eventSourcing = eventSourcing;
            this.State = this.Clone(rayGrain.State);
        }
        public TState State { get; }
        public async Task<bool> Commit()
        {
            if (this.IsProcessed())
                transactionState =  TransactionState.Commit;
            if (transactionEvents.Count > 0)
            {
                //Save all events in this transaction
                if (await this.eventSourcing.SaveAsync(transactionEvents))
                {
                    //Turn off transaction status in the primary service
                    this.rayGrain.EndTransaction(transactionEvents);
                    foreach (var e in publishEvents)
                    {
                        await this.rayGrain.PublishEventAsync(e);
                    }
                    return true;
                }
            }
            return false;
        }
   
        public void Rollback()
        {
            if (this.IsProcessed())
            {
                transactionState = TransactionState.Rollback;
            }

            transactionEvents.Clear();
            publishEvents.Clear();
            this.rayGrain.EndTransaction();
        }
        public void WriteEventAsync(IEvent<TStateKey> @event, bool isPublish = true)
        {
            if (@event == null)
            {
                throw new ArgumentNullException("WriteEventAsync event cannot be empty");
            }
            @event.Version = State.NextVersion();
            @event.StateId = State.StateId;
            this.State.Player(@event);
            transactionEvents.Add(@event);
            if (isPublish)
            {
                publishEvents.Add(@event);
            }
        }
        private bool IsProcessed()
        {
            if (this.transactionState == TransactionState.NotCommit)
                return true;
            else if (this.transactionState == TransactionState.Commit)
                throw new Exception("The transaction has been commit");
            else if (this.transactionState == TransactionState.Rollback)
                throw new Exception("The transaction has been rolled back");
            else
                throw new Exception("The transaction has been processed");
        }
        /// <summary>
        /// 克隆实体
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        public TState Clone(TState state)
        {
            MemoryStream stream = new MemoryStream();
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(stream, this);
            stream.Position = 0;
            return (TState)formatter.Deserialize(stream);
        }
    }

    public enum TransactionState
    {
        NotCommit = 0,
        Commit=1,
        Rollback=2
    }


}
