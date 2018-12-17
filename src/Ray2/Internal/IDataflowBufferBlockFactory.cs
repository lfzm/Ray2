using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Ray2.Internal
{
    public interface IDataflowBufferBlockFactory
    {
        IDataflowBufferBlock<TData> Create<TData>(string name, Func<BufferBlock<IDataflowBufferWrap<TData>>, Task> processor)
            where TData : class;
    }
}
