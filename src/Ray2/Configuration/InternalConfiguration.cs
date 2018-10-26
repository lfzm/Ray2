using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ray2.EventProcess;

namespace Ray2.Configuration
{
    public class InternalConfiguration : IInternalConfiguration
    {
        internal Dictionary<string, EventProcessOptions> _eventProcessOptions = new Dictionary<string, EventProcessOptions>();
        internal Dictionary<string, EventProcessOptions> _eventProcessOptionsByProcessor = new Dictionary<string, EventProcessOptions>();
        internal Dictionary<string, EventSourceOptions> _eventSourceOptions = new Dictionary<string, EventSourceOptions>();
        internal Dictionary<string, EventSourceOptions> _eventSourceOptionsBySource = new Dictionary<string, EventSourceOptions>();
        internal Dictionary<string, EventPublishOptions> _eventPublishOptions = new Dictionary<string, EventPublishOptions>();
        internal Dictionary<string, EventInfo> _eventInfos = new Dictionary<string, EventInfo>();
        public EventInfo GetEvenInfo(string name)
        {
            if (_eventInfos.TryGetValue(name, out EventInfo eventInfo))
            {
                return eventInfo;
            }
            else
                return null;
        }

        public bool GetEvenType(string name, out Type type)
        {
            var info = this.GetEvenInfo(name);
            if (info != null)
            {
                type = info.Type;
                return true;
            }
            else
            {
                type = null;
                return false;
            }
        }
        public IList<EventInfo> GetEvenInfoList()
        {
            return _eventInfos.Values.ToList();
        }

        public EventProcessOptions GetEventProcessOptions(string name)
        {
            if (_eventProcessOptions.TryGetValue(name, out EventProcessOptions options))
            {
                return options;
            }
            else
            {
                return null;
            }
        }
        public EventProcessOptions GetEventProcessOptions(IEventProcessor eventProcessor)
        {
            if (_eventProcessOptionsByProcessor.TryGetValue(eventProcessor.GetType().FullName, out EventProcessOptions options))
            {
                return options;
            }
            else
            {
                return null;
            }
        }

        public IList<EventProcessOptions> GetEventProcessOptionsList()
        {
            return _eventProcessOptions.Values.ToList(); ;
        }

        public EventPublishOptions GetEventPublishOptions(IRay ray)
        {
            if (_eventPublishOptions.TryGetValue(ray.GetType().FullName, out EventPublishOptions options))
            {
                return options;
            }
            else
            {
                return null;
            }
        }

        public EventPublishOptions GetEventPublishOptions(IEventProcessor eventProcessor)
        {
            if (_eventPublishOptions.TryGetValue(eventProcessor.GetType().FullName, out EventPublishOptions options))
            {
                return options;
            }
            else
            {
                return null;
            }
        }

        public EventSourceOptions GetEventSourceOptions(string name)
        {
            if (_eventSourceOptions.TryGetValue(name, out EventSourceOptions options))
            {
                return options;
            }
            else
            {
                return null;
            }
        }

        public EventSourceOptions GetEventSourceOptions(IRay ray)
        {
            if (_eventSourceOptionsBySource.TryGetValue(ray.GetType().FullName, out EventSourceOptions options))
            {
                return options;
            }
            else
            {
                return null;
            }
        }

        public IList<EventSourceOptions> GetEventSourceOptions()
        {
            return _eventSourceOptions.Values.ToList();
        }


    }
}
