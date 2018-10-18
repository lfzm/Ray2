using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Ray2.EventProcess
{
    public class EPBufferBlock : IEPBufferBlock
    {
        private readonly Func<BufferBlock<IEvent>, Task> _process;
        private readonly BufferBlock<IEvent> eventBufferBlock = new BufferBlock<IEvent>();
        private int isProcessing = 0;

        public EPBufferBlock(Func<BufferBlock<IEvent>, Task> process)
        {
            this._process = process;
        }
        public Task SendAsync(IEvent @event)
        {
            return Task.Run(async () =>
            {
                var result = await eventBufferBlock.SendAsync(@event);
                if (isProcessing == 0)
                    TriggerEventProcess();
                return result;
            });
        }

        private async void TriggerEventProcess()
        {
            await Task.Run(async () =>
            {
                if (Interlocked.CompareExchange(ref isProcessing, 1, 0) == 1)
                    return;
                try
                {
                    while (await eventBufferBlock.OutputAvailableAsync())
                    {
                        await this._process(eventBufferBlock);
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
