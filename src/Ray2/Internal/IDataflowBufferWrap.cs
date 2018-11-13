using System.Threading.Tasks;

namespace Ray2.Internal
{
    public interface IDataflowBufferWrap
    {
         TaskCompletionSource<bool> TaskSource { get; }

    }
}
