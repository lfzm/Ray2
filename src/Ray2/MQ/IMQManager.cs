using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ray2.MQ
{
    public interface IMQManager
    {
        /// <summary>
        /// Start mq subscription
        /// </summary>
        /// <param name="subscribeList"></param>
        /// <returns></returns>
        Task Start(IList<EventSubscribeInfo> subscribeList);
    }
}
