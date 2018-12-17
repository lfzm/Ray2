using Orleans.Runtime;
using Ray2.MQ;
using Ray2.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ray2.EventSource
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
        private readonly ISerializer serializer;
        private List<EventTransactionModel<TStateKey>> transactionEvents = new List<EventTransactionModel<TStateKey>>();
        public TransactionState TransactionState { get; private set; } = TransactionState.NotCommit;
        public EventTransaction(RayGrain<TState, TStateKey> rayGrain, IServiceProvider serviceProvider, IEventSourcing<TState, TStateKey> eventSourcing)
        {
            this.rayGrain = rayGrain;
            this.serializer = serviceProvider.GetRequiredServiceByName<ISerializer>(SerializationType.JsonUTF8);
            this.eventSourcing = eventSourcing;
            this.State = this.Clone(rayGrain.State);
        }
        public TState State { get; }
        public async Task<bool> Commit()
        {
            if (this.IsProcessed())
                TransactionState = TransactionState.Commit;
            if (this.Count() > 0)
            {
                //Save all events in this transaction
                var events = transactionEvents.Select(f => f.Event).ToList();
                if (await this.eventSourcing.SaveAsync(events))
                {
                    //Turn off transaction status in the primary service
                    this.rayGrain.EndTransaction(events);
                    List<Task> publishTasks = transactionEvents.Select(f => this.rayGrain.PublishEventAsync(f.Event, f.PublishType)).ToList();
                    Task.WaitAll(publishTasks.ToArray(), TimeSpan.FromSeconds(30));
                    return true;
                }
            }
            return false;
        }
        public void Rollback()
        {
            if (this.IsProcessed())
            {
                TransactionState = TransactionState.Rollback;
            }
            transactionEvents.Clear();
            this.rayGrain.EndTransaction();
        }

        public void WriteEventAsync(EventTransactionModel<TStateKey> model)
        {
            if (model == null)
            {
                throw new ArgumentNullException("WriteEventAsync event cannot be empty");
            }
            transactionEvents.Add(model);
            this.State.Player(model.Event);
        }

        public void WriteEventAsync(IEvent<TStateKey> @event, MQPublishType publishType = MQPublishType.Asynchronous)
        {
            if (@event == null)
            {
                throw new ArgumentNullException("WriteEventAsync event cannot be empty");
            }
            @event.Version = State.NextVersion();
            @event.StateId = State.StateId;
            var model = new EventTransactionModel<TStateKey>(@event, publishType);
            this.WriteEventAsync(model);
        }

        public void WriteEventAsync(IList<IEvent<TStateKey>> events, MQPublishType publishType = MQPublishType.Asynchronous)
        {
            if (events == null || events.Count == 0)
            {
                throw new ArgumentNullException("WriteEventAsync event cannot be empty");
            }
            foreach (var e in events)
            {
                this.WriteEventAsync(e, publishType);
            }
        }

        private bool IsProcessed()
        {
            if (this.TransactionState == TransactionState.NotCommit)
                return true;
            else if (this.TransactionState == TransactionState.Commit)
                throw new Exception("The transaction has been commit");
            else if (this.TransactionState == TransactionState.Rollback)
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
            var bytes = this.serializer.Serialize(state);
            return this.serializer.Deserialize<TState>(bytes);
        }
        public int Count()
        {
            return this.transactionEvents.Count;
        }


    }

    public enum TransactionState
    {
        NotCommit = 0,
        Commit = 1,
        Rollback = 2
    }


}
