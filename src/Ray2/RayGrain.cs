using Orleans;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Ray2.EventSourcing;
using Microsoft.Extensions.Logging;
using System.Runtime.ExceptionServices;
using Ray2.Storage;
using Newtonsoft.Json;
using Ray2.Configuration;
using Microsoft.Extensions.Options;
using System.Runtime.CompilerServices;
using Ray2.MQ;
using Orleans.Runtime;

namespace Ray2
{
    /// <summary>
    /// This is the Ray Grain base class
    /// </summary>
    public abstract class RayGrain<TState, TStateKey> : Grain, IRay
        where TState : IState<TStateKey>, new()
    {
        protected internal TState State { get; private set; }
        protected ILogger Logger { get; set; }
        internal IEventSourcing<TState, TStateKey> eventSourcing;
        internal IMQPublisher mqPublisher;
        private EventSourcingOptions config;
        private bool IsBeginTransaction;
        private bool IsBlock;
        protected abstract TStateKey StateId { get; }
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
                this.eventSourcing = await this.ServiceProvider.GetRequiredServiceByKey<Type, IEventSourcing<TState, TStateKey>>(this.GetType()).Init(this.StateId);
                this.mqPublisher = this.ServiceProvider.GetRequiredService<IMQPublisher>();

                this.State = await this.eventSourcing.ReadSnapshotAsync();
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
            await this.eventSourcing.SaveSnapshotAsync(this.State);
            await base.OnDeactivateAsync();
        }

        /// <summary>
        /// Write event
        /// </summary>
        /// <param name="event"><see cref="IEvent{TStateKey}"/></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected async virtual Task<bool> WriteEventAsync(IEvent<TStateKey> @event, bool isPublish = true)
        {
            if (@event == null)
                throw new ArgumentNullException("WriteEventAsync event cannot be empty");

            this.IsBlockProcess();
            if (this.IsBeginTransaction)
                throw new Exception("Do not process a single event in a transaction");

            @event.Version = State.NextVersion();
            @event.StateId = State.StateId;
            //Storage event
            if (await this.eventSourcing.SaveAsync(@event))
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
                if (isPublish)
                    await this.PublishEventAsync(@event);
                //Save snapshot
                await this.eventSourcing.SaveSnapshotAsync(this.State);
                return true;
            }
            else
                return false;
        }
        /// <summary>
        /// Event Publish to mq
        /// </summary>
        /// <param name="event"><see cref="IEvent"/></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual Task<bool> PublishEventAsync(IEvent @event)
        {
            if (@event == null)
                throw new ArgumentNullException("PublishEventAsync event cannot be empty");
            return mqPublisher.Publish(@event);
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
            return new EventTransaction<TState, TStateKey>(this, this.ServiceProvider);
        }
        /// <summary>
        /// end transaction
        /// </summary>
        internal void EndTransaction(IList<IEvent> events = null)
        {
            this.IsBeginTransaction = false;
            //Play master status
            if (events != null && events.Count > 0)
            {
                this.State = this.eventSourcing.TraceAsync(this.State, events);
                this.eventSourcing.SaveSnapshotAsync(this.State).Wait();
            }
        }
        private void IsBlockProcess()
        {
            if (this.IsBlock)
                throw new Exception($"Event version and state version don't match!,StateId={State.StateId},Event Version={State.NextVersion()},State Version={State.Version}");
        }
    }
}
