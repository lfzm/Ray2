using System;
using System.Threading.Tasks;

namespace Ray2.Internal
{
    public interface IDataflowBufferBlock<T> : IDataflowBufferBlock
    {
        Task<bool> SendAsync(T t);
    }

    public interface IDataflowBufferBlock
    {
        int Count { get; }
    }
}
