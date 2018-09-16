using Ray2.Exceptions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ray2.Configuration
{
    internal static class RayConfig
    {
        internal static Dictionary<string, EventSourcesConfig> EventSources { get; } = new Dictionary<string, EventSourcesConfig>();
        internal static Dictionary<string, EventProcessConfig> EventProcessors { get; } = new Dictionary<string, EventProcessConfig>();
        internal static List<MQSubscribeConfig> MQSubscribes { get; } = new List<MQSubscribeConfig>();
        private static Dictionary<Type, string> EventSourcingsConfigMap { get; } = new Dictionary<Type, string>();
        private static Dictionary<Type, string> EventProcessorsConfigMap { get; } = new Dictionary<Type, string>();
        public static EventSourcesConfig GetEventSourceConfig(Type type)
        {
            if (EventSourcingsConfigMap.TryGetValue(type, out string eventSourceName))
            {
                return EventSources[eventSourceName];
            }
            else
                return null;
        }

        public static EventSourcesConfig GetEventSourceConfig(string  eventSourceName)
        {
            return EventSources[eventSourceName];
        }
        public static EventProcessConfig GetEventProcessConfig(Type type)
        {
            if (EventProcessorsConfigMap.TryGetValue(type, out string groupName))
            {
                return EventProcessors[groupName];
            }
            else
                throw new RayConfigException($"{type.FullName} is not configured EventSourcing，use the EventSubscribeConfig configuration. ");
        }


    }
}
