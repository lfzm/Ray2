using Ray2.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ray2.MQ
{
    /// <summary>
    /// this is the event publish  interface
    /// </summary>
    public interface IMQPublisher
    {
        /// <summary>
        /// Publish a message to the message queue
        /// </summary>
        /// <param name="warp">Send event to MQ package</param>
        /// <returns></returns>
        Task<bool> PublishAsync(EventPublishBufferWrap warp);
        /// <summary>
        ///  Publish a message to the message queue
        /// </summary>
        /// <param name="evt">Event object</param>
        /// <param name="topic">This is the message queue topic</param>
        /// <param name="mqProviderName">This is a message queue provider</param>
        /// <param name="publishType"> MQ publish type</param>
        /// <returns></returns>
        Task<bool> PublishAsync(IEvent evt, string topic, string mqProviderName, MQPublishType publishType = MQPublishType.Asynchronous);
    }
}
