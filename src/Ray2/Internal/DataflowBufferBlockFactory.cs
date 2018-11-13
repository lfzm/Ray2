using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Ray2.Internal
{
    public class DataflowBufferBlockFactory : IDataflowBufferBlockFactory
    {
        ConcurrentDictionary<string, IDataflowBufferBlock> DataflowBufferBlocks = new ConcurrentDictionary<string, IDataflowBufferBlock>();
        public IDataflowBufferBlock<T> Create<T>(string name, Func<BufferBlock<T>, Task> processor)
        {
            return (IDataflowBufferBlock<T>)DataflowBufferBlocks.GetOrAdd(name, (key) =>
            {
                return new DataflowBufferBlock<T>(processor);
            });
        }
    }
}
