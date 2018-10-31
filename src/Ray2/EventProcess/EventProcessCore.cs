using Microsoft.Extensions.Logging;
using Ray2.Configuration;
using Ray2.EventSource;
using Ray2.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Ray2.EventProcess
{
    using EventProcessor = Func<IEvent, Task>;
    public class EventProcessCore : IEventProcessCore
    {
        protected readonly ILogger _logger;
        protected readonly IEventProcessBufferBlock _eventProcessBufferBlock;
        protected readonly IServiceProvider _serviceProvider;
        protected EventProcessor _eventProcessor;
        public EventProcessOptions Options { get;  set; }

        public EventProcessCore(IServiceProvider serviceProvider, ILogger<EventProcessCore> logger)
        {
            this._serviceProvider = serviceProvider;
            this._logger = logger;
            this._eventProcessBufferBlock = new EventProcessBufferBlock(this.TriggerEventProcessing);
        }

        public Task<IEventProcessCore> Init(EventProcessor eventProcessor)
        {
            this._eventProcessor = eventProcessor;
            IEventProcessCore eventProcessCore = this;
            return Task.FromResult(eventProcessCore);
        }

        public Task Tell(IEvent @event)
        {
            return this._eventProcessBufferBlock.SendAsync(@event);
        }

        protected virtual async Task TriggerEventProcessing(BufferBlock<IEvent> eventBuffer)
        {
            try
            {
                List<IEvent> events = new List<IEvent>();
                while (eventBuffer.TryReceive(out var @event))
                {
                    if (events.Count > this.Options.OnceProcessCount)
                        break;
                    events.Add(@event);
                }
                //Exclude duplicate events
                using (var tokenSource = new CancellationTokenSource())
                {
                    var tasks = events.Select(@event => this._eventProcessor(@event));
                    var taskAllEvent = Task.WhenAll(tasks);
                    using (var taskTimeOut = Task.Delay(this.Options.OnceProcessTimeout, tokenSource.Token))
                    {
                        await Task.WhenAny(taskAllEvent, taskTimeOut);
                        if (taskAllEvent.Status == TaskStatus.RanToCompletion)
                        {
                            tokenSource.Cancel();
                        }
                        else
                        {
                            throw new Exception("Event processing timeout");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, "event process failed");
            }
        }

    }

    public class EventProcessCore<TState, TStateKey> : EventProcessCore, IEventProcessCore<TState, TStateKey>
           where TState : IState<TStateKey>, new()
    {
        private readonly IEventSourcing _eventSourcing;
        private TState State;
        private TStateKey StateId;
        private IStateStorage _stateStorage;
        private string StorageTable;
        public EventProcessCore(IServiceProvider serviceProvider, ILogger<EventProcessCore<TState, TStateKey>> logger)
            : base(serviceProvider, logger)
        {
            this._eventSourcing = serviceProvider.GetEventSourcing(this.Options.EventSourceName);
        }

        public async Task<IEventProcessCore<TState, TStateKey>> Init(TStateKey stateId, EventProcessor eventProcessor)
        {
            this.StateId = stateId;
            this._eventProcessor = eventProcessor;
            var storageFactory = new StorageFactory(this._serviceProvider, this.Options.StatusOptions);
            this._stateStorage = await storageFactory.GetStateStorage(this.Options.ProcessorName, StorageType.EventProcessState, this.StateId.ToString());
            this.StorageTable = await storageFactory.GetTable(this.Options.ProcessorName, StorageType.EventProcessState, this.StateId.ToString());
            this.State = await this.ReadStateAsync();
            return this;
        }
        protected override async Task TriggerEventProcessing(BufferBlock<IEvent> eventBuffer)
        {
            try
            {
                if (this.Options.OnceProcessCount > 1)
                {
                    List<IEvent> events = new List<IEvent>();
                    while (eventBuffer.TryReceive(out var @event))
                    {
                        if (events.Count > this.Options.OnceProcessCount)
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
                this._logger.LogError(ex, "event process failed");
            }
        }
        private async Task TriggerEventProcess(IEvent @event)
        {
            if (@event.Version < State.NextVersion())
            {
                this._logger.LogError($"{this.StateId}+{@event.Version} Repeated execution of events");
                return;
            }
            if (@event.Version > State.NextVersion())
            {
                //Get missing events
                var events = await this._eventSourcing.GetListAsync(new EventQueryModel(State.Version, @event.Version, @event.Timestamp));
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
                await this._eventProcessor(@event);
                this.State.Player(@event);
                if (this.Options.StatusOptions.StatusMode == StatusMode.Synchronous)
                    await this.SaveStateAsync();
            }
        }
        private async Task TriggerEventProcess(List<IEvent> events)
        {
            var lastEvent = events.Last();
            if (this.State.Version + events.Count != lastEvent.Version)
            {
                events = await this._eventSourcing.GetListAsync(new EventQueryModel(State.Version, lastEvent.Version, lastEvent.Timestamp)) as List<IEvent>;
                if (events == null || events.Count == lastEvent.Version - this.State.Version)
                    throw new Exception("Event lost");
            }
            using (var tokenSource = new CancellationTokenSource())
            {
                var tasks = events.Select(@event => this._eventProcessor(@event));
                var taskAllEvent = Task.WhenAll(tasks);
                using (var taskTimeOut = Task.Delay(this.Options.OnceProcessTimeout, tokenSource.Token))
                {
                    await Task.WhenAny(taskAllEvent, taskTimeOut);
                    if (taskAllEvent.Status == TaskStatus.RanToCompletion)
                    {
                        tokenSource.Cancel();
                        this.State.Player(lastEvent);
                        await this.SaveStateAsync();
                    }
                    else
                    {
                        throw new Exception("Event processing timeout");
                    }
                }
            }
        }
        public async Task SaveStateAsync()
        {
            if (this.State.Version == 1)
                await this._stateStorage.InsertAsync(this.StorageTable, this.StateId, State);
            else
                await this._stateStorage.UpdateAsync(this.StorageTable, this.StateId, State);
        }
        public async Task<TState> ReadStateAsync()
        {
            if (this.State != null)
                return this.State;
            var state = await this._stateStorage.ReadAsync<TState>(this.StorageTable, this.StateId);
            if (state == null)
            {
                return new TState()
                {
                    StateId = this.StateId
                };
            }
            else
                return State;
        }
        public Task ClearStateAsync()
        {
            return this._stateStorage.DeleteAsync(this.StorageTable, this.StateId);
        }
      
    }
}
