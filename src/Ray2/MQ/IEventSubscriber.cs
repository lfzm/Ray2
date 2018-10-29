using Ray2.MQ;
using System.Threading.Tasks;

namespace Ray2.MQ
{
    /// <summary>
    /// this is the  subscribe interface
    /// </summary>
    public interface IEventSubscriber
    {
        Task Subscribe(EventSubscribeInfo info);
    }
}
