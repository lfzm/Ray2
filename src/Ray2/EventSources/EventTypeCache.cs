using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ray2.EventSources
{
    public  class EventTypeCache: IEventTypeCache
    {
        private  Dictionary<string, Type> EventTypes { get; } = new Dictionary<string, Type>();

        public  bool GetEventType(string name, out Type value)
        {
            if (EventTypes.TryGetValue(name, out var type))
            {
                value = type;
                return true;
            }
            else
            {
                value = null;
                return false;
            }
        }
        public  void Initialize()
        {
            var assemblyList = AppDomain.CurrentDomain.GetAssemblies().Where(a => !a.IsDynamic);
            var eventType = typeof(IEvent);
            foreach (var assembly in assemblyList)
            {
                var allType = assembly.GetExportedTypes().Where(t => eventType.IsAssignableFrom(t)
                && t.IsClass == true
                && t.IsAbstract == false
                && t.GetConstructors().Any(c => c.GetParameters().Length == 0));
                foreach (var type in allType)
                {
                    if (Activator.CreateInstance(type) is IEvent msg)
                    {
                        if (!string.IsNullOrEmpty(msg.TypeCode))
                            EventTypes.Add(msg.TypeCode, type);
                        else
                            EventTypes.Add(type.FullName, type);
                    }
                }
            }
        }
    }
}
