using System.Threading.Tasks;

namespace Ray2.Internal
{
    public interface IDataflowBufferBlock<TData> : IDataflowBufferBlock
    {
        Task<bool> SendAsync(TData data);
        Task<bool> SendAsync(TData data,bool isWallHandle);
    }

    public interface IDataflowBufferBlock
    {
        int Count { get; }
    }
}
