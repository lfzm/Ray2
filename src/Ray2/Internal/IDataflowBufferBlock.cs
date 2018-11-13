using System;
using System.Threading.Tasks;

namespace Ray2.Internal
{
    public interface IDataflowBufferBlock<T> : IDataflowBufferBlock 
        where T : IDataflowBufferWrap
    {
        Task<bool> SendAsync(T t);
    }

    public interface IDataflowBufferBlock
    {
 
    }
}
