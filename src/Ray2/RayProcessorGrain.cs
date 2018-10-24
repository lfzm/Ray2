using Orleans;
using Orleans.Runtime;
using Ray2.EventProcess;
using Ray2.MQ;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Ray2
{
    /// <summary>
    /// This is the event process grain
    /// </summary>
    public abstract class RayProcessorGrain<TState, TStateKey> : Grain, IEventProcessor
           where TState : IState<TStateKey>, new()
    {
        protected TState State { get { return _eventProcessCore.ReadStateAsync().GetAwaiter().GetResult(); } }
        protected abstract TStateKey StateId { get; }
        protected IMQPublisher MQPublisher { get; private set; }
        private IEventProcessCore<TState, TStateKey> _eventProcessCore;
        public override async Task OnActivateAsync()
        {
            this._eventProcessCore = await this.ServiceProvider.GetRequiredServiceByName<IEventProcessCore<TState, TStateKey>>(this.GetType().FullName)
                .Init(this.StateId, this.OnEventProcessing);
            this.MQPublisher = this.ServiceProvider.GetRequiredServiceByName<IMQPublisher>(this.GetType().FullName);
            await base.OnActivateAsync();
        }
        public override async Task OnDeactivateAsync()
        {
            await this._eventProcessCore.SaveStateAsync();
            await base.OnDeactivateAsync();
        }
        public Task Tell(IEvent @event)
        {
            return this._eventProcessCore.Tell(@event);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public abstract Task OnEventProcessing(IEvent @event);
    }
    public abstract class RayProcessorGrain<TStateKey> : RayProcessorGrain<EventProcessState<TStateKey>, TStateKey>
    {
    
    }

}
