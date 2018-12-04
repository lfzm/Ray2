using Ray2.EventProcess;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ray2.Configuration
{
    /// <summary>
    /// Ray system internal configuration
    /// </summary>
    public interface IInternalConfiguration
    {
        /// <summary>
        /// Get the event type by name
        /// </summary>
        /// <param name="name">The name of the event</param>
        /// <param name="type">event type</param>
        /// <returns></returns>
        bool GetEvenType(string name, out Type type);
        /// <summary>
        /// Get event information by name
        /// </summary>
        /// <param name="name">The name of the event</param>
        /// <returns></returns>
        EventInfo GetEvenInfo(string name);
        /// <summary>
        /// Get all event types
        /// </summary>
        /// <returns></returns>
        IList<EventInfo> GetEvenInfoList();
        /// <summary>
        /// Get the configuration of the event processor based on the event processor name
        /// </summary>
        /// <param name="name">The name of the event processor</param>
        /// <returns></returns>
        EventProcessOptions GetEventProcessOptions(string name);
        /// <summary>
        /// Get the event processor configuration based on the event processor
        /// </summary>
        /// <param name="eventProcessor">processor</param>
        /// <returns></returns>
        EventProcessOptions GetEventProcessOptions(IEventProcessor eventProcessor);
        /// <summary>
        /// Get all processor configurations
        /// </summary>
        /// <returns></returns>
        IList<EventProcessOptions> GetEventProcessOptionsList();
        /// <summary>
        /// Get the event source configuration based on the event source name
        /// </summary>
        /// <param name="name">The name of the event source</param>
        /// <returns></returns>
        EventSourceOptions GetEventSourceOptions(string name);
        /// <summary>
        /// Get the event source configuration based on the event source
        /// </summary>
        /// <param name="ray">Event source</param>
        /// <returns></returns>
        EventSourceOptions GetEventSourceOptions(IRay ray);
        /// <summary>
        /// Get all event source configurations
        /// </summary>
        /// <returns></returns>
        IList<EventSourceOptions> GetEventSourceOptions();
        /// <summary>
        /// Get the event publishing configuration in the event source
        /// </summary>
        /// <param name="ray">Event source</param>
        /// <returns></returns>
        EventPublishOptions GetEventPublishOptions(IRay ray);
        /// <summary>
        ///  Get the event publishing configuration in the event processor
        /// </summary>
        /// <param name="@event">event</param>
        /// <returns></returns>
        EventPublishOptions GetEventPublishOptions(IEvent @event);
        /// <summary>
        /// Get all event publishing configurations
        /// </summary>
        /// <returns></returns>
        IList<EventPublishOptions> GetEventPublishOptionsList();
    }
}
