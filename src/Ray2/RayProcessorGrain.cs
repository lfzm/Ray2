using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orleans;
using Ray2.Configuration;
using Ray2.EventHandle;
using Ray2.EventHandler;
using Ray2.EventProcess;
using Ray2.EventSource;
using Ray2.Exceptions;
using Ray2.MQ;
using Ray2.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Ray2
{
    /// <summary>
    /// This is the event process grain
    /// </summary>
    public abstract class RayProcessorGrain<TState, TStateKey> : Grain, IEventProcessor
           where TState : IState<TStateKey>, new()
    {
        protected TState State { get; private set; }
        protected ILogger Logger { get; set; }
        protected IMQPublisher MQPublisher { get; private set; }
        protected abstract TStateKey StateId { get; }
        private EventProcessingOptions config;
        private EventSourcingOptions eventSourcesConfig;
        private IEventTraceability<TState, TStateKey> eventTraceability;
        private IEventSourcing<TState, TStateKey> eventSourcing;
        private IEventProcessBufferBlock eventProcessBufferBlock;
        public RayProcessorGrain(ILogger logger)
        {
            this.Logger = logger;
        }
        public override async Task OnActivateAsync()
        {
            this.config = RayConfig.GetEventProcessConfig(this.GetType());
            this.eventSourcesConfig = RayConfig.GetEventSourceConfig(this.config.EventSourceName);

            this.MQPublisher = this.ServiceProvider.GetRequiredService<IMQPublisher>();
            this.eventSourcing = this.ServiceProvider.GetRequiredService<IEventSourcing<TState, TStateKey>>();
            this.eventTraceability = this.ServiceProvider.GetRequiredService<IEventTraceability<TState, TStateKey>>();
            this.eventProcessBufferBlock = new EventProcessBufferBlock(this.TriggerEventProcess);
            this.eventTraceability.Injection(StateId, this.config.Snapshot);
            this.eventSourcing.Injection(StateId, this.eventSourcesConfig);
            this.State = await this.eventTraceability.ReadSnapshotAsync();
            await base.OnActivateAsync();
        }
        public override async Task OnDeactivateAsync()
        {
            await this.eventTraceability.SaveSnapshotAsync(this.State);
            await base.OnDeactivateAsync();
        }
        public Task Tell(IEvent @event)
        {
            return this.eventProcessBufferBlock.SendAsync(@event);
        }
        internal async Task TriggerEventProcess(BufferBlock<IEvent> eventBuffer)
        {
            try
            {
                if (this.config.OnceProcessCount > 1)
                {
                    List<IEvent> events = new List<IEvent>();
                    while (eventBuffer.TryReceive(out var @event))
                    {
                        if (events.Count > this.config.OnceProcessCount)
                            break;
                        if (@event.Version < this.State.NextVersion())
                            continue;
                        events.Add(@event);
                    }
                    //Exclude duplicate events
                    events = events.OrderBy(w => w.Version).ToList();
                    await this.TriggerEventProcess(events);
                }
                else
                {
                    while (eventBuffer.TryReceive(out var @event))
                    {
                        await this.TriggerEventProcess(@event);
                    }
                }
            }
            catch (Exception ex)
            {
                this.Logger.LogError(ex, "event process failed");
            }
        }
        internal async Task TriggerEventProcess(IEvent @event)
        {
            if (@event.Version < State.NextVersion())
            {
                this.Logger.LogError($"{this.StateId}+{@event.Version} Repeated execution of events");
                return;
            }
            if (@event.Version > State.NextVersion())
            {
                //Get missing events
                var events = await this.eventSourcing.GetListAsync(new EventQueryModel(State.Version, @event.Version, @event.Timestamp));
                if (events == null ||
                    @event.Version - State.NextVersion() != events.OrderBy(w => w.Version).Count() ||
                    events.First().Version != State.NextVersion())
                {
                    throw new Exception($"Event version of the error,Type={GetType().FullName},StateId={this.StateId.ToString()},StateVersion={State.Version},EventVersion={@event.Version}");
                }
                //handle events
                foreach (var evt in events)
                {
                    await this.TriggerEventProcess(evt);
                }
            }
            else
            {
                await this.OnEventProcessing(@event);
                this.State.Player(@event);
                if (this.config.Snapshot.SnapshotType == SnapshotType.Synchronous)
                    await this.eventTraceability.SaveSnapshotAsync(this.State);
            }
        }
        internal async Task TriggerEventProcess(List<IEvent> events)
        {
            var lastEvent = events.Last();
            if (this.State.Version + events.Count != lastEvent.Version)
            {
                events = await this.eventSourcing.GetListAsync(new EventQueryModel(State.Version, lastEvent.Version, lastEvent.Timestamp)) as List<IEvent>;
                if (events == null || events.Count == lastEvent.Version - this.State.Version)
                    throw new Exception("Event lost");
            }
            using (var tokenSource = new CancellationTokenSource())
            {
                var tasks = events.Select(@event => OnEventProcessing(@event));
                var taskAllEvent = Task.WhenAll(tasks);
                using (var taskTimeOut = Task.Delay(this.config.OnceProcessTimeout, tokenSource.Token))
                {
                    await Task.WhenAny(taskAllEvent, taskTimeOut);
                    if (taskAllEvent.Status == TaskStatus.RanToCompletion)
                    {
                        tokenSource.Cancel();
                        this.State.Player(lastEvent);
                        await this.eventTraceability.SaveSnapshotAsync(this.State);
                    }
                    else
                    {
                        throw new Exception("Event processing timeout");
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public abstract Task OnEventProcessing(IEvent @event);
    }
    public abstract class RayProcessorGrain<TStateKey> : RayProcessorGrain<EventProcessState<TStateKey>, TStateKey>
    {
        public RayProcessorGrain(ILogger logger) : base(logger)
        {

        }
    }

}
