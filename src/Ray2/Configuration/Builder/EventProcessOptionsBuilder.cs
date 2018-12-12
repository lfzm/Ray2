using Orleans;
using System;
using System.Collections.Generic;

namespace Ray2.Configuration.Builder
{
    public class EventProcessOptionsBuilder
    {
        private string _processorName;
        private string _processorFullName;
        private string _eventSourceName;
        private int _onceProcessCount;
        private TimeSpan _onceProcessTimeout;
        private StatusOptions _statusOptions;
        private IList<EventSubscribeOptions> _eventSubscribeOptions;
        private ProcessorType _processorType;
        private Type _processorHandle;
        public EventProcessOptionsBuilder WithProcessor(string name, Type type)
        {
            this._processorHandle = type;
            this._processorName = name;
            this._processorFullName = type.FullName;
            if (typeof(Grain).IsAssignableFrom(type))
            {
                _processorType = ProcessorType.GrainProcessor;
            }
            else
            {
                _processorType = ProcessorType.SimpleProcessor;
            }
            return this;
        }

        public EventProcessOptionsBuilder WithEventSourceName(string name)
        {
            this._eventSourceName = name;
            return this;
        }

        public EventProcessOptionsBuilder WithOnceProcessConfig(int onceProcessCount, TimeSpan onceProcessTimeout)
        {
            this._onceProcessCount = onceProcessCount;
            this._onceProcessTimeout = onceProcessTimeout;
            return this;
        }

        public EventProcessOptionsBuilder WithStatusOptions(StatusOptions statusOptions)
        {
            this._statusOptions = statusOptions;
            return this;
        }

        public EventProcessOptionsBuilder WithEventSubscribeOptions(IList<EventSubscribeOptions> eventSubscribeOptions)
        {
            this._eventSubscribeOptions = eventSubscribeOptions;
            return this;
        }

        public EventProcessOptions Build()
        {
            return new EventProcessOptions(_processorName, _processorFullName, _processorType, _processorHandle, _eventSourceName, _onceProcessCount, _onceProcessTimeout, _statusOptions, _eventSubscribeOptions);
        }
    }
}
