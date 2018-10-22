using Microsoft.Extensions.Logging;
using Orleans.Runtime;
using Ray2.EventProcess;
using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Ray2
{
    public abstract class RayProcessor : IEventProcessor
    {
        protected ILogger Logger { get; set; }
        private readonly IEventProcessCore _eventProcessCore;
        private readonly IServiceProvider _serviceProvider;
        public RayProcessor(IServiceProvider serviceProvider, ILogger logger)
        {
            this._serviceProvider = serviceProvider;
            this._eventProcessCore =  this._serviceProvider.GetRequiredServiceByName<IEventProcessCore>(this.GetType().FullName)
                .Init(this.OnEventProcessing).GetAwaiter().GetResult();
            this.Logger = logger;
        }
        public Task Tell(IEvent @event)
        {
            return this._eventProcessCore.Tell(@event);
        }
      
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public abstract Task OnEventProcessing(IEvent @event);

    }
}
