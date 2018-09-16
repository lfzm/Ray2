using Microsoft.Extensions.Logging;
using Ray2.Storage;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Ray2.EventSources
{
    public class EventBufferBlock : IEventBufferBlock
    {
        private readonly IEventStorage eventStorage;
        private readonly ILogger logger;
        private readonly string providerName;
        private readonly BufferBlock<EventBufferWrap> EventFlowChannel = new BufferBlock<EventBufferWrap>();
        private int isProcessing = 0;

        public EventBufferBlock(string providerName, ILogger<EventBufferBlock> logger, IEventStorage eventStorage)
        {
            this.providerName = providerName;
            this.logger = logger;
            this.eventStorage = eventStorage;
        }

        public Task<bool> SendAsync(IEventStorageModel @event)
        {
            return Task.Run(async () =>
            {
                var bufferWrap = new EventBufferWrap(@event);
                await EventFlowChannel.SendAsync(bufferWrap);
                if (isProcessing == 0)
                     TriggerSaveEventProcess();
                return await bufferWrap.TaskSource.Task;
            });
        }

        private async void TriggerSaveEventProcess()
        {
            await Task.Run(async () =>
            {
                if (Interlocked.CompareExchange(ref isProcessing, 1, 0) == 1)
                    return;
                try
                {
                    int count = 0;
                    while (await EventFlowChannel.OutputAvailableAsync())
                    {
                        var bufferWrapList = new List<EventBufferWrap>();
                        while (EventFlowChannel.TryReceive(out var evt))
                        {
                            count += evt.Value.Count();
                            bufferWrapList.Add(evt);
                            if (count >= 500) break;//Process up to 500 items at a time
                        }
                        try
                        {
                            await eventStorage.SaveAsync(bufferWrapList);
                        }
                        catch (Exception ex)
                        {
                            bufferWrapList.ForEach(f => f.TaskSource.SetException(ex));
                        }
                    }
                }
                finally
                {
                    Interlocked.Exchange(ref isProcessing, 0);
                }
            }).ConfigureAwait(false);
        }

    }
}
