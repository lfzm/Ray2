using System;
using System.Threading.Tasks;

namespace Ray2.Internal
{
    public interface IDataflowBufferBlock<T> : IDataflowBufferBlock
    {
        Task<bool> SendAsync(T t);
        Task<bool> SendAsync(T t,bool isWallHandle);
    }

    public interface IDataflowBufferBlock
    {
        int Count { get; }
    }
}
