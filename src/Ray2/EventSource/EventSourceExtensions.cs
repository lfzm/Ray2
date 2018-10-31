using Microsoft.Extensions.DependencyInjection;
using Ray2.Configuration;
using System;

namespace Ray2.EventSource
{
    public static class EventSourceExtensions
    {
        public static IEventSourcing GetEventSourcing(this IServiceProvider serviceProvider, string eventSourceName)
        {
            var internalConfiguration = serviceProvider.GetRequiredService<IInternalConfiguration>();
            var options = internalConfiguration.GetEventSourceOptions(eventSourceName);
            return serviceProvider.GetEventSourcing(options);
        }

        public static IEventSourcing GetEventSourcing(this IServiceProvider serviceProvider, IRay ray)
        {
            var internalConfiguration = serviceProvider.GetRequiredService<IInternalConfiguration>();
            var options = internalConfiguration.GetEventSourceOptions(ray);
            return serviceProvider.GetEventSourcing(options);
        }

        private static IEventSourcing GetEventSourcing(this IServiceProvider serviceProvider, EventSourceOptions options)
        {
            var eventSourcing = serviceProvider.GetRequiredService<IEventSourcing>();
            eventSourcing.Options = options;
            return eventSourcing;
        }

        public static IEventSourcing<TState, TStateKey> GetEventSourcing<TState, TStateKey>(this IServiceProvider serviceProvider, string eventSourceName)
             where TState : IState<TStateKey>, new()
        {
            var internalConfiguration = serviceProvider.GetRequiredService<IInternalConfiguration>();
            var options = internalConfiguration.GetEventSourceOptions(eventSourceName);
            return serviceProvider.GetEventSourcing<TState, TStateKey>(options);
        }

        public static IEventSourcing<TState, TStateKey> GetEventSourcing<TState, TStateKey>(this IServiceProvider serviceProvider, IRay ray)
             where TState : IState<TStateKey>, new()
        {
            var internalConfiguration = serviceProvider.GetRequiredService<IInternalConfiguration>();
            var options = internalConfiguration.GetEventSourceOptions(ray);
            return serviceProvider.GetEventSourcing<TState, TStateKey>(options);
        }

        private static IEventSourcing<TState, TStateKey> GetEventSourcing<TState, TStateKey>(this IServiceProvider serviceProvider, EventSourceOptions options)
             where TState : IState<TStateKey>, new()
        {
            var eventSourcing = serviceProvider.GetRequiredService<IEventSourcing<TState, TStateKey>>();
            eventSourcing.Options = options;
            return eventSourcing;
        }
    }
}
