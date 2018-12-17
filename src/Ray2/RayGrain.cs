using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orleans;
using Ray2.Configuration;
using Ray2.EventSource;
using Ray2.Internal;
using Ray2.MQ;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Ray2
{
    /// <summary>
    /// This is the Ray Grain base class
    /// </summary>
    public abstract class RayGrain<TState, TStateKey> : Grain, IRay
        where TState : IState<TStateKey>, new()
    {
        public TState State { get; private set; }
        protected abstract TStateKey StateId { get; }
        protected ILogger Logger { get; set; }
        private IDataflowBufferBlock<EventTransactionModel<TStateKey>> _eventBufferBlock;
        private IEventSourcing<TState, TStateKey> _eventSourcing;
        private IInternalConfiguration _internalConfiguration;
        private IMQPublisher _mqPublisher;
        private bool IsBeginTransaction;
        private bool IsBlock;
        private EventPublishOptions PublishOptions;
        public RayGrain(ILogger logger)
        {
            this.Logger = logger;
        }
        /// <summary>
        /// Activate Grain
        /// </summary>
        /// <returns></returns>
        public override async Task OnActivateAsync()
        {
            try
            {
                this._eventBufferBlock = new DataflowBufferBlock<EventTransactionModel<TStateKey>>(this.TriggerEventStorage);
                this._internalConfiguration = this.ServiceProvider.GetRequiredService<IInternalConfiguration>();
                this._mqPublisher = this.ServiceProvider.GetRequiredService<IMQPublisher>();
                this._eventSourcing = await this.ServiceProvider.GetEventSourcing<TState, TStateKey>(this).Init(this.StateId);
                this.State = await this._eventSourcing.ReadSnapshotAsync();
                this.PublishOptions = this._internalConfiguration.GetEventPublishOptions(this);
                await base.OnActivateAsync();
            }
            catch (Exception ex)
            {
                this.Logger.LogError(ex, $"{StateId} Activate Grain failure");
                throw ex;
            }
        }
        public override async Task OnDeactivateAsync()
        {
            await this._eventSourcing.SaveSnapshotAsync(this.State);
            await base.OnDeactivateAsync();
        }

        /// <summary>
        /// Write event
        /// </summary>
        /// <param name="event"><see cref="IEvent{TStateKey}"/></param>
        /// <param name="publishType">is to publish to MQ</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected async virtual Task<bool> WriteAsync(IEvent<TStateKey> @event, MQPublishType publishType = MQPublishType.Asynchronous)
        {
            if (@event == null)
                throw new ArgumentNullException("WriteAsync event cannot be empty");

            this.IsBlockProcess();
            if (this.IsBeginTransaction)
                throw new Exception("Do not process a single event in a transaction");

            @event.Version = State.NextVersion();
            @event.StateId = State.StateId;
            //Storage event
            if (await this._eventSourcing.SaveAsync(@event))
            {
                try
                {
                    //Paly state
                    this.State.Player(@event);
                }
                catch (Exception ex)
                {
                    this.IsBlock = true;
                    throw ex;
                }
                //Publish event
                await this.PublishEventAsync(@event, publishType);

                //Save snapshot
                if (this._eventSourcing.Options.SnapshotOptions.SnapshotType == SnapshotType.Synchronous)
                {
                    await this._eventSourcing.SaveSnapshotAsync(this.State);
                }
                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// concurrent  write event
        /// </summary>
        /// <param name="event"><see cref="IEvent{TStateKey}"/></param>
        /// <param name="publishType">is to publish to MQ</param>
        /// <returns></returns>
        protected virtual Task<bool> ConcurrentWriteAsync(IEvent<TStateKey> @event, MQPublishType publishType = MQPublishType.Asynchronous)
        {
            if (@event == null)
                throw new ArgumentNullException("ConcurrentWriteAsync event cannot be empty");
            this.IsBlockProcess();
            var model = new EventTransactionModel<TStateKey>(@event, publishType);
            return this._eventBufferBlock.SendAsync(model);
        }
        private Task TriggerEventStorage(BufferBlock<IDataflowBufferWrap<EventTransactionModel<TStateKey>>> eventBuffer)
        {
            var transaction = this.BeginTransaction();
            List<IDataflowBufferWrap<EventTransactionModel<TStateKey>>> events = new List<IDataflowBufferWrap<EventTransactionModel<TStateKey>>>();
            while (eventBuffer.TryReceive(out var model))
            {
                events.Add(model);
                transaction.WriteEventAsync(model.Data.Event, model.Data.PublishType);
                if (transaction.Count() >= 1000)
                    break;
            }
            try
            {
                transaction.Commit();
                events.ForEach(evt => evt.CompleteHandler(true));
            }
            catch (Exception ex)
            {
                this.Logger.LogError(ex, "Concurrent write event exception");
                transaction.Rollback();
                events.ForEach(evt => evt.ExceptionHandler(ex));
            }
            return Task.CompletedTask;
        }

        /// <summary>
        /// Event Publish to mq
        /// </summary>
        /// <param name="event"><see cref="IEvent"/></param>
        /// <returns></returns>
        internal async Task PublishEventAsync(IEvent @event, MQPublishType publishType = MQPublishType.Asynchronous)
        {
            if (publishType == MQPublishType.NotPublish)
                return;
            try
            {
                if (@event == null)
                    throw new ArgumentNullException("PublishEventAsync event cannot be empty");
                if (this.PublishOptions != null)
                {
                    await _mqPublisher.PublishAsync(@event, this.PublishOptions.Topic, this.PublishOptions.MQProvider, publishType);
                }
                var opt = this._internalConfiguration.GetEventPublishOptions(@event);
                if (opt != null)
                {
                    await _mqPublisher.PublishAsync(@event, this.PublishOptions.Topic, this.PublishOptions.MQProvider, publishType);
                }
            }
            catch (Exception ex)
            {
                this.Logger.LogError(ex, $"Publish {@event.TypeCode}.V{@event.Version} event failed");
            }
        }
        /// <summary>
        /// begin transaction
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual IEventTransaction<TState, TStateKey> BeginTransaction()
        {
            this.IsBlockProcess();
            if (this.IsBeginTransaction)
                throw new Exception("Unable to open event again during transaction");
            this.IsBeginTransaction = true;
            return new EventTransaction<TState, TStateKey>(this, this.ServiceProvider, this._eventSourcing);
        }
        /// <summary>
        /// end transaction
        /// </summary>
        internal void EndTransaction(IList<IEvent<TStateKey>> events = null)
        {
            this.IsBeginTransaction = false;
            //Play master status
            if (events != null && events.Count > 0)
            {
                this.State.Player(events);
                this._eventSourcing.SaveSnapshotAsync(this.State).Wait(10000);
            }
        }
        private void IsBlockProcess()
        {
            if (this.IsBlock)
            {
                throw new Exception($"Event version and state version don't match!,StateId={State.StateId},Event Version={State.NextVersion()},State Version={State.Version}");
            }
        }
    }
}
