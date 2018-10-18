using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Ray2.MQ;

namespace Ray2.EventSource
{
    /// <summary>
    /// Transaction batch event
    /// </summary>
    /// <typeparam name="TState"><see cref="IState{TStateKey}"/></typeparam>
    /// <typeparam name="TStateKey"></typeparam>
    public class EventTransaction<TState, TStateKey> : IEventTransaction<TState, TStateKey>, IDisposable
            where TState : IState<TStateKey>, new()
    {
        private readonly RayGrain<TState, TStateKey> rayGrain;
        private readonly IEventSourcing<TState, TStateKey> eventSourcing;
        private readonly IMQPublisher mqPublisher;
        private readonly IServiceProvider serviceProvider;

        private IList<IEvent<TStateKey>> transactionEvents = new List<IEvent<TStateKey>>();
        private IList<IEvent> publishEvents = new List<IEvent>();
        private int transactionState = 0;
        public EventTransaction(RayGrain<TState, TStateKey> rayGrain, IServiceProvider serviceProvider)
        {
            this.rayGrain = rayGrain;
            this.eventSourcing = rayGrain.eventSourcing;
            this.mqPublisher = rayGrain.mqPublisher;
            this.serviceProvider = serviceProvider;
            this.State = this.Clone(rayGrain.State);
        }

        public TState State { get; }
        public async Task<bool> Commit()
        {
            if (this.IsProcessed())
                transactionState = 1;
            if (transactionEvents.Count > 0)
            {
                //Save all events in this transaction
                if (await this.eventSourcing.SaveAsync(transactionEvents))
                {
                    //Turn off transaction status in the primary service
                    this.rayGrain.EndTransaction(transactionEvents);
                    await this.mqPublisher.Publish(publishEvents);
                    return true;
                }
            }
            return false;
        }
        public void Dispose()
        {
            this.rayGrain.EndTransaction();
        }
        public void Rollback()
        {
            if (this.IsProcessed())
                transactionState = 2;

            transactionEvents.Clear();
            publishEvents.Clear();
            this.rayGrain.EndTransaction();
        }
        public void WriteEventAsync(IEvent<TStateKey> @event, bool isPublish = true)
        {
            if (@event == null)
                throw new ArgumentNullException("WriteEventAsync event cannot be empty");

            @event.Version = State.NextVersion();
            @event.StateId = State.StateId;
            this.State.Player(@event);
            transactionEvents.Add(@event);
            if (isPublish)
                publishEvents.Add(@event);
        }
        private bool IsProcessed()
        {
            if (this.transactionState == 0)
                return true;
            else if (this.transactionState == 1)
                throw new Exception("The transaction has been commit");
            else if (this.transactionState == 2)
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


}
