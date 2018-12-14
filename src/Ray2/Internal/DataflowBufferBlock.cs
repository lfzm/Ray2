using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Ray2.Internal
{
    public class DataflowBufferBlock<T> : IDataflowBufferBlock<T>
    {
        private int isProcessing = 0;
        private readonly BufferBlock<T> dataflowChannel = new BufferBlock<T>();
        private readonly Func<BufferBlock<T>, Task> processor;

        public int Count => dataflowChannel.Count;

        public DataflowBufferBlock(Func<BufferBlock<T>, Task> processor)
        {
            this.processor = processor;
        }
        public Task<bool> SendAsync(T wrap, bool isWallHandle)
        {
            return Task.Run(async () =>
            {
                //First use the synchronous method to quickly write to the BufferBlock, if the failure is using the asynchronous method
                if (!dataflowChannel.Post(wrap))
                {
                    if (!await dataflowChannel.SendAsync(wrap))
                    {
                        return false;
                    }
                }
                if (isProcessing == 0)
                    TriggerProcessor();
                if (!isWallHandle)
                    return true;
                //Determine if you need to wait for processing
                if (wrap is IDataflowBufferWrap wr)
                {
                    return await wr.TaskSource.Task;
                }
                else
                {
                    return true;
                }
            });
        }
        public Task<bool> SendAsync(T wrap)
        {
            return this.SendAsync(wrap, true);
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
