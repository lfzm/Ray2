using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Ray2.Configuration;
using System;

namespace Ray2.EventProcess
{
    public class EventProcessorFactory : IEventProcessorFactory
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IInternalConfiguration _configuration;
        private readonly ILogger _logger;
        public EventProcessorFactory(IInternalConfiguration configuration, IServiceProvider serviceProvider, ILogger<EventProcessorFactory> logger)
        {
            this._configuration = configuration;
            this._serviceProvider = serviceProvider;
            this._logger = logger;
        }
        public IEventProcessor Create(string processName)
        {
            var options = this._configuration.GetEventProcessOptions(processName);
            if (options == null)
            {
                throw new Exception($"Did not find the configuration of the {processName} processor ");
            }
            if (options.ProcessorType == ProcessorType.GrainProcessor)
            {
                return new EventProcessorGrainDispatch(options.ProcessorFullName, _serviceProvider);
            }
            else
            {
                return (IEventProcessor)_serviceProvider.GetRequiredService(options.ProcessorHandle);
            }
        }
    }
}
