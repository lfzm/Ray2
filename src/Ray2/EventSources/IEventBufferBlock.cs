using Ray2.Storage;
using System.Threading.Tasks;

namespace Ray2.EventSources
{
    public interface IEventBufferBlock
    {
        Task<bool> SendAsync(IEventStorageModel @event);

    }
}
