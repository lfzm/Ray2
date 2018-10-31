using Microsoft.Extensions.DependencyInjection;
using Ray2.Configuration;
using System;

namespace Ray2.EventProcess
{
    public static class EventProcessExtensions
    {
        public static IEventProcessCore GetEventProcessCore(this IServiceProvider serviceProvider, string ProcessorName)
        {
            var internalConfiguration = serviceProvider.GetRequiredService<IInternalConfiguration>();
            var options = internalConfiguration.GetEventProcessOptions(ProcessorName);
            return serviceProvider.GetEventProcessCore(options);
        }

        public static IEventProcessCore GetEventProcessCore(this IServiceProvider serviceProvider, IEventProcessor eventProcessor)
        {
            var internalConfiguration = serviceProvider.GetRequiredService<IInternalConfiguration>();
            var options = internalConfiguration.GetEventProcessOptions(eventProcessor);
            return serviceProvider.GetEventProcessCore(options);
        }
        private static IEventProcessCore GetEventProcessCore(this IServiceProvider serviceProvider, EventProcessOptions options)
        {
            var eventProcessCore = serviceProvider.GetRequiredService<IEventProcessCore>();
            eventProcessCore.Options = options;
            return eventProcessCore;
        }

        public static IEventProcessCore<TState, TStateKey> GetEventProcessCore<TState, TStateKey>(this IServiceProvider serviceProvider, string ProcessorName)
                where TState : IState<TStateKey>, new()
        {
            var internalConfiguration = serviceProvider.GetRequiredService<IInternalConfiguration>();
            var options = internalConfiguration.GetEventProcessOptions(ProcessorName);
            return serviceProvider.GetEventProcessCore<TState, TStateKey>(options);
        }

        public static IEventProcessCore<TState, TStateKey> GetEventProcessCore<TState, TStateKey>(this IServiceProvider serviceProvider, IEventProcessor eventProcessor)
                where TState : IState<TStateKey>, new()
        {
            var internalConfiguration = serviceProvider.GetRequiredService<IInternalConfiguration>();
            var options = internalConfiguration.GetEventProcessOptions(eventProcessor);
            return serviceProvider.GetEventProcessCore<TState, TStateKey>(options);
        }

        private static IEventProcessCore<TState, TStateKey> GetEventProcessCore<TState, TStateKey>(this IServiceProvider serviceProvider, EventProcessOptions options)
                where TState : IState<TStateKey>, new()
        {
            var eventProcessCore = serviceProvider.GetRequiredService<IEventProcessCore<TState, TStateKey>>();
            eventProcessCore.Options = options;
            return eventProcessCore;
        }
    }
}
