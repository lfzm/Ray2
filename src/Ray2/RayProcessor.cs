using Ray2.EventProcess;
using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Ray2
{
    public abstract class RayProcessor : IEventProcessor
    {
        private readonly IEventProcessCore _eventProcessCore;
        protected readonly IServiceProvider ServiceProvider;
        public RayProcessor(IServiceProvider serviceProvider)
        {
            this.ServiceProvider = serviceProvider;
            this._eventProcessCore = this.ServiceProvider.GetEventProcessCore(this).Init(this.OnEventProcessing).GetAwaiter().GetResult();
        }
        public Task Tell(IEvent @event)
        {
            return this._eventProcessCore.Tell(@event);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public abstract Task OnEventProcessing(IEvent @event);

    }
}
