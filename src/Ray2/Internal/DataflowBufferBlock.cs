using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Ray2.Internal
{
    public class DataflowBufferBlock<T> : IDataflowBufferBlock<T>
          where T : IDataflowBufferWrap
    {
        private int isProcessing = 0;
        private readonly BufferBlock<T> dataflowChannel = new BufferBlock<T>();
        private readonly Func<BufferBlock<T>, Task> processor;

        public DataflowBufferBlock(Func<BufferBlock<T>, Task> processor)
        {
            this.processor = processor;
        }
        public Task<bool> SendAsync(T wrap)
        {
            return Task.Run(async () =>
            {
                var result = await dataflowChannel.SendAsync(wrap);
                if (isProcessing == 0)
                    TriggerProcessor();
                return await wrap.TaskSource.Task;
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
                    while (await dataflowChannel.OutputAvailableAsync())
                    {
                        await this.processor(dataflowChannel);
                    }
                }
                finally
                {
                    Interlocked.Exchange(ref isProcessing, 0);
                }
            });

        }



    }
}
