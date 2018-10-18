using Ray2.MQ;
using System.Threading.Tasks;

namespace Ray2.EventProcess
{
    public interface IEPDispatch
    {
        Task<bool> Notice(EventSubscribeInfo info, EventPublishMessage message);
    }
}
