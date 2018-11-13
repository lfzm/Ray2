using Ray2.Configuration;
using System;
using System.Threading.Tasks;

namespace Ray2.EventProcess
{
    using EventProcessor = Func<IEvent, Task>;
    public interface IEventProcessCore
    {
        EventProcessOptions Options { get; set; }
        Task<IEventProcessCore> Init(EventProcessor eventProcessor);
        Task Tell(EventProccessBufferWrap eventWrap);
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
