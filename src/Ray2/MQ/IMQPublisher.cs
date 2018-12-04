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
        /// <param name="topic">topic</param>
        /// <param name="model">Event object</param>
        /// <returns></returns>
        Task<bool> Publish(IEvent @event);

        /// <summary>
        /// Publish a message to the message queue
        /// </summary>
        /// <param name="topic">topic</param>
        /// <param name="model">Event object collection</param>
        /// <returns></returns>
        Task Publish(IList<IEvent> events);

        /// <summary>
        /// Publish a message to the message queue
        /// </summary>
        /// <param name="topic">topic</param>
        /// <param name="model">Event object</param>
        /// <param name="topic">This is the message queue topic</param>
        /// <param name="mqProviderName">This is a message queue provider</param>
        /// <returns></returns>
        Task<bool> Publish(IEvent @event,string topic, string mqProviderName);
        /// <summary>
        /// Publish a message to the message queue
        /// </summary>
        /// <param name="topic">topic</param>
        /// <param name="model">Event object collection</param>
        /// <param name="topic">This is the message queue topic</param>
        /// <param name="mqProviderName">This is a message queue provider</param>
        /// <returns></returns>
        Task Publish(IList<IEvent> events, string topic, string mqProviderName);
    }
}
