using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Ray2.EventProcess
{
    using EventProcessor = Func<IEvent, Task>;
    public interface IEventProcessCore
    {
        Task<IEventProcessCore> Init(EventProcessor eventProcessor);

        Task Tell(IEvent @event);
    }


    public interface IEventProcessCore<TState, TStateKey> : IEventProcessCore
    where TState : IState<TStateKey>, new()
    {
        Task<IEventProcessCore<TState, TStateKey>> Init(TStateKey stateKey, EventProcessor eventProcessor);

        Task SaveStateAsync();
        Task<TState> ReadStateAsync();
        Task ClearStateAsync();
    }
}
