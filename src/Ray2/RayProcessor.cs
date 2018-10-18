using Microsoft.Extensions.Logging;
using Ray2.Configuration;
using Ray2.EventHandler;
using Ray2.EventProcess;
using Ray2.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Ray2
{
    public abstract class RayProcessor : IEventProcessor
    {
        protected ILogger Logger { get; set; }
        private IEventProcessBufferBlock eventProcessBufferBlock;
        private EventProcessingOptions eventProcessConfig;
        public RayProcessor(ILogger logger)
        {
            this.eventProcessConfig = RayConfig.GetEventProcessConfig(this.GetType());
            if (this.eventProcessConfig == null)
                throw new RayConfigException($"{this.GetType().FullName} is not configured EventSourcing，use the EventSubscribeConfig configuration. ");
            this.Logger = logger;
            this.eventProcessBufferBlock = new EventProcessBufferBlock(this.TriggerEventProcess);
        }
        public Task Tell(IEvent @event)
        {
            return this.eventProcessBufferBlock.SendAsync(@event);
        }
        internal async Task TriggerEventProcess(BufferBlock<IEvent> eventBuffer)
        {
            try
            {
                List<IEvent> events = new List<IEvent>();
                while (eventBuffer.TryReceive(out var @event))
                {
                    if (events.Count > this.eventProcessConfig.OnceProcessCount)
                        break;
                    events.Add(@event);
                }
                //Exclude duplicate events
                using (var tokenSource = new CancellationTokenSource())
                {
                    var tasks = events.Select(@event => OnEventProcessing(@event));
                    var taskAllEvent = Task.WhenAll(tasks);
                    using (var taskTimeOut = Task.Delay(this.eventProcessConfig.OnceProcessTimeout, tokenSource.Token))
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
                this.Logger.LogError(ex, "event process failed");
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public abstract Task OnEventProcessing(IEvent @event);

    }
}
