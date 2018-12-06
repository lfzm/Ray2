using Ray2.EventProcess;
using Ray2.RabbitMQ.Configuration;
using System.Threading.Tasks;

namespace Ray2.RabbitMQ
{
    /// <summary>
    /// This is the consumer interface of Rebbit.
    /// </summary>
    public interface IRabbitConsumer
    {
        string Queue { get; }
        string Exchange { get; }
        RabbitConsumeOptions Options { get; }
        IEventProcessor Processor { get; }
        /// <summary>
        /// Subscribe to RabbitMQ
        /// </summary>
        /// <returns></returns>
        Task Subscribe(string queue, string exchange, IEventProcessor processor, RabbitConsumeOptions options);
        /// <summary>
        /// Is this consumer available?
        /// </summary>
        /// <returns></returns>
        bool IsAvailable();
        /// <summary>
        /// Need to expand
        /// </summary>
        /// <returns></returns>
        bool IsExpand();
        /// <summary>
        /// Stop listening
        /// </summary>
        /// <returns></returns>
        Task Close();

    }
}
