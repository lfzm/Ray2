using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Ray2.Internal
{
    public class DataflowBufferBlock<T> : IDataflowBufferBlock<T>
    {
        private int isProcessing = 0;
        private CancellationToken cancellationToken;
        private readonly BufferBlock<T> dataflowChannel = new BufferBlock<T>();
        private readonly Func<BufferBlock<T>, Task> processor;

        public DataflowBufferBlock(Func<BufferBlock<T>, Task> processor)
        {
            this.cancellationToken = CancellationToken.None;
            this.processor = processor;
        }
        public Task<bool> SendAsync(T t)
        {
            return Task.Run(async () =>
            {
                var result = await dataflowChannel.SendAsync(t);
                if (isProcessing == 0)
                    TriggerProcessor();
                return result;
            });
        }

        public async void TriggerProcessor()
        {
            await Task.Run(async () =>
            {
                if (Interlocked.CompareExchange(ref isProcessing, 1, 0) == 1)
                    return;
                try
                {
                    while (await dataflowChannel.OutputAvailableAsync(cancellationToken))
                    {
                        await this.processor(dataflowChannel);
                    }
                }
                finally
                {
                    Interlocked.Exchange(ref isProcessing, 0);
                }
            }).ConfigureAwait(false);

        }

        public void Canceled()
        {
            cancellationToken = new CancellationToken(true);
        }
    }
}
