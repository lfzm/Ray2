using Ray2.EventProcess;
using Ray2.EventSource;
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
        public Task<bool> Tell(EventModel model)
        {
            return this._eventProcessCore.Tell(model);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public abstract Task OnEventProcessing(IEvent @event);

    }
}
