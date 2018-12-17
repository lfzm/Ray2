using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Ray2.Internal
{
    public class DataflowBufferBlock<TData> : IDataflowBufferBlock<TData>
    {
        private int isProcessing = 0;
        private readonly BufferBlock<IDataflowBufferWrap<TData>> dataflowChannel = new BufferBlock<IDataflowBufferWrap<TData>>();
        private readonly Func<BufferBlock<IDataflowBufferWrap<TData>>, Task> processor;

        public int Count => dataflowChannel.Count;

        public DataflowBufferBlock(Func<BufferBlock<IDataflowBufferWrap<TData>>, Task> processor)
        {
            this.processor = processor;
        }
        public Task<bool> SendAsync(TData data, bool isWallHandle)
        {
            return Task.Run(async () =>
            {
                var wrap = new DataflowBufferWrap<TData>(data);
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
                return await wrap.Wall();
            });
        }
        public Task<bool> SendAsync(TData data)
        {
            return this.SendAsync(data, true);
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
