using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ray2.MQ
{
    public interface IMQSubscriber
    {
        /// <summary>
        /// Start MQ subscription
        /// </summary>
        /// <returns></returns>
        Task Start();
    }
}
