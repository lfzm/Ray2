using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orleans.Runtime;
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
        protected readonly EventProcessOptions Options;
        protected EventProcessor _eventProcessor;

        public EventProcessCore(IServiceProvider serviceProvider, EventProcessOptions options, ILogger<EventProcessCore> logger)
        {
            this._serviceProvider = serviceProvider;
            this._logger = logger;
            this.Options = options;
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
        protected readonly IEventSourcing _eventSourcing;
        public TState State;
        private TStateKey StateId;
        private IStorageSharding _stateStorageSharding;
        private IEventStorage _stateStorage;
        public EventProcessCore(IServiceProvider serviceProvider, EventProcessOptions options, ILogger<EventProcessCore<TState, TStateKey>> logger)
            : base(serviceProvider, options, logger)
        {
            this._eventSourcing = this.GetEventSourcing();
        }

        public async Task<IEventProcessCore<TState, TStateKey>> Init(TStateKey stateId, EventProcessor eventProcessor)
        {
            this.StateId = stateId;
            this._eventProcessor = eventProcessor;

            return this;
        }

        public Task SaveStateAsync()
        {
            throw new NotImplementedException();
        }

        public Task<TState> ReadStateAsync()
        {
            throw new NotImplementedException();
        }

        public Task ClearStateAsync()
        {
            throw new NotImplementedException();
        }

        private IEventSourcing GetEventSourcing()
        {
            var eventSourceOptions = this._serviceProvider.GetRequiredServiceByName<EventSourceOptions>(this.Options.EventSourceName);
            var logger = this._serviceProvider.GetRequiredService<ILogger<EventSourcing>>();
            return new EventSourcing(this._serviceProvider, eventSourceOptions, logger);
        }
    }
}
