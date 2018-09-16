using System.Threading.Tasks;

namespace Ray2.MQ
{
    public interface IMQManager
    {
        /// <summary>
        /// start MQ
        /// </summary>
        /// <returns></returns>
        Task Start();
    }
}
