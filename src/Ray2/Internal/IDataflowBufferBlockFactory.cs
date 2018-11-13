using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Ray2.Internal
{
    public interface IDataflowBufferBlockFactory
    {
        IDataflowBufferBlock<T> Create<T>(string name, Func<BufferBlock<T>, Task> processor, CancellationToken token);

        IDataflowBufferBlock<T> Create<T>(string name, Func<BufferBlock<T>, Task> processor);
    }
}
