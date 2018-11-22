using Orleans;
using Orleans.Runtime;
using Ray2.EventProcess;
using Ray2.EventSource;
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
        private IEventProcessCore<TState, TStateKey> _eventProcessCore;
        protected TState State { get { return _eventProcessCore.ReadStateAsync().GetAwaiter().GetResult(); } }
        protected abstract TStateKey StateId { get; }
        protected IMQPublisher MQPublisher { get; private set; }
        public override async Task OnActivateAsync()
        {
            this._eventProcessCore = await this.ServiceProvider.GetEventProcessCore<TState, TStateKey>(this)
                .Init(this.StateId, this.OnEventProcessing);
            this.MQPublisher = this.ServiceProvider.GetRequiredServiceByName<IMQPublisher>(this.GetType().FullName);
            await base.OnActivateAsync();
        }
        public override async Task OnDeactivateAsync()
        {
            await this._eventProcessCore.SaveStateAsync();
            await base.OnDeactivateAsync();
        }
        public Task<bool> Tell(EventModel model)
        {
            return this._eventProcessCore.Tell(model);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public abstract Task OnEventProcessing(IEvent @event);
    }
    public abstract class RayProcessorGrain<TStateKey> : RayProcessorGrain<EventProcessState<TStateKey>, TStateKey>
    {

    }

}
