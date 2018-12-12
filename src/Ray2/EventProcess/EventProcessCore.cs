using Microsoft.Extensions.Logging;
using Ray2.Configuration;
using Ray2.EventSource;
using Ray2.Internal;
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
        protected readonly IDataflowBufferBlock<EventModel> _eventBufferBlock;
        protected readonly IServiceProvider _serviceProvider;
        protected EventProcessor _eventProcessor;
        public EventProcessOptions Options { get; set; }

        public EventProcessCore(IServiceProvider serviceProvider, ILogger<EventProcessCore> logger)
        {
            this._serviceProvider = serviceProvider;
            this._logger = logger;
            this._eventBufferBlock = new DataflowBufferBlock<EventModel>(this.TriggerEventProcessing);
        }

        public Task<IEventProcessCore> Init(EventProcessor eventProcessor)
        {
            this._eventProcessor = eventProcessor;
            IEventProcessCore eventProcessCore = this;
            return Task.FromResult(eventProcessCore);
        }

        public virtual async Task<bool> Tell(EventModel model)
        {
            //Wait for the number of buffers to be greater than twice the number of processing
            if (this._eventBufferBlock.Count > this.Options.OnceProcessCount * 2)
            {
                await Task.Delay(TimeSpan.FromSeconds(1));
            }
            return await this._eventBufferBlock.SendAsync(model);
        }

        protected virtual async Task TriggerEventProcessing(BufferBlock<EventModel> eventBuffer)
        {
            try
            {
                List<IEvent> events = new List<IEvent>();
                while (eventBuffer.TryReceive(out var model))
                {
                    events.Add(model.Event);
                    if (events.Count > this.Options.OnceProcessCount)
                        break;
                }
                //Exclude duplicate events
                using (var tokenSource = new CancellationTokenSource())
                {
                    var tasks = events.Select(e => this._eventProcessor(e));
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
        private IEventSourcing _eventSourcing;
        private TState State;
        private TStateKey StateId;
        private IStateStorage _stateStorage;
        private string StorageTable;
        private int exeEventCount;
        public EventProcessCore(IServiceProvider serviceProvider, ILogger<EventProcessCore<TState, TStateKey>> logger)
            : base(serviceProvider, logger)
        {
        }

        public async Task<IEventProcessCore<TState, TStateKey>> Init(TStateKey stateId, EventProcessor eventProcessor)
        {
            this.StateId = stateId;
            this._eventProcessor = eventProcessor;
            this._eventSourcing = this._serviceProvider.GetEventSourcing(this.Options.EventSourceName);
            var storageFactory = new StorageFactory(this._serviceProvider, this.Options.StatusOptions);
            this._stateStorage = await storageFactory.GetStateStorage(this.Options.ProcessorName, StorageType.EventProcessState, this.StateId.ToString());
            this.StorageTable = await storageFactory.GetTable(this.Options.ProcessorName, StorageType.EventProcessState, this.StateId.ToString());
            this.State = await this.ReadStateAsync();
            return this;
        }

        public override Task<bool> Tell(EventModel model)
        {
            return this._eventBufferBlock.SendAsync(model);
        }
        protected override async Task TriggerEventProcessing(BufferBlock<EventModel> eventBuffer)
        {
            try
            {
                if (this.Options.OnceProcessCount > 1)
                {
                    List<IEvent> events = new List<IEvent>();
                    while (eventBuffer.TryReceive(out var model))
                    {
                        if (model.Version < this.State.NextVersion())
                            continue;
                        events.Add(model.Event);
                        if (events.Count > this.Options.OnceProcessCount)
                            break;

                    }
                    //Exclude duplicate events
                    events = events.OrderBy(w => w.Version).ToList();
                    await this.TriggerEventProcessBatch(events);
                }
                else
                {
                    while (eventBuffer.TryReceive(out var model))
                    {
                        await this.TriggerEventProcess(model.Event);
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
                return;

            if (@event.Version > State.NextVersion())
            {
                //Get missing events
                var events = await this._eventSourcing.GetListAsync(new EventQueryModel(State.Version, @event.Version, @event.Timestamp));
                if (events == null ||
                    @event.Version - State.Version != events.OrderBy(w => w.Version).Count() ||
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
                await this.LazySaveStateAsync();
            }
        }
        private async Task TriggerEventProcess(List<IEvent> events)
        {
            if (events.Count == 0)
                return;
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
                        this.State.Player(events);
                        await this.SaveStateAsync();
                    }
                    else
                    {
                        throw new Exception($"{this.Options.ProcessorName} processing event timeout");
                    }
                }
            }
        }
        private async Task TriggerEventProcessBatch(List<IEvent> events)
        {
            if (events == null || events.Count == 0)
                return;

            var lastEvent = events.Last();
            if (this.State.Version + events.Count != lastEvent.Version)
            {
                var queryModel = new EventQueryModel(State.Version, lastEvent.Version, lastEvent.Timestamp)
                {
                    StateId = this.StateId
                };
                events = await this._eventSourcing.GetListAsync(queryModel) as List<IEvent>;
                events = events.OrderBy(f => f.Version).ToList();
                if (events == null || events.Count != lastEvent.Version - this.State.Version)
                    throw new Exception("Event lost");
            }
            //Group execution. Avoid processing timeouts
            if (this.Options.OnceProcessCount < events.Count)
            {
                for (int i = 0; i < (events.Count / (this.Options.OnceProcessCount) + 1); i++)
                {
                    var list = events.Skip((i * this.Options.OnceProcessCount) - 1).Take(this.Options.OnceProcessCount).ToList();
                    await this.TriggerEventProcess(list);
                }
            }
            else
            {
                await this.TriggerEventProcess(events);
            }


        }
   
        public async Task LazySaveStateAsync()
        {
            if (this.Options.StatusOptions.StatusMode == StatusMode.Synchronous)
                await this.SaveStateAsync();
            else if (this.Options.StatusOptions.StatusMode == StatusMode.Asynchronous)
            {
                if (this.exeEventCount > 200)
                    await this.SaveStateAsync();
                else
                    this.exeEventCount++;
            }
        }
        public async Task SaveStateAsync()
        {
            if (this.State.Version == 0)
                await this._stateStorage.InsertAsync(this.StorageTable, this.StateId, State);
            else
                await this._stateStorage.UpdateAsync(this.StorageTable, this.StateId, State);
        }
        public async Task<TState> ReadStateAsync()
        {
            if (this.State != null)
                return this.State;
            this.State = await this._stateStorage.ReadAsync<TState>(this.StorageTable, this.StateId);
            if (this.State == null)
            {
                this.State = new TState()
                {
                    StateId = this.StateId
                };
                await this.SaveStateAsync();
            }
            return this.State;
        }
        public Task ClearStateAsync()
        {
            return this._stateStorage.DeleteAsync(this.StorageTable, this.StateId);
        }

    }
}
