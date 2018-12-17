using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Ray2.Internal
{
    public class DataflowBufferBlockFactory : IDataflowBufferBlockFactory
    {
        ConcurrentDictionary<string, IDataflowBufferBlock> DataflowBufferBlocks = new ConcurrentDictionary<string, IDataflowBufferBlock>();
        public IDataflowBufferBlock<TData> Create<TData>(string name, Func<BufferBlock<IDataflowBufferWrap<TData>>, Task> processor)
              where TData : class
        {
            return (IDataflowBufferBlock<TData>)DataflowBufferBlocks.GetOrAdd(name, (key) =>
            {
                return new DataflowBufferBlock<TData>(processor);
            });
        }
    }
}
