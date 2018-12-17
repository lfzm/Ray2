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
        /// <param name="model">Send event to MQ publish model</param>
        /// <returns></returns>
        Task<bool> PublishAsync(EventPublishModel model);
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
