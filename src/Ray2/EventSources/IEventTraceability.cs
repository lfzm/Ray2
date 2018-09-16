using Ray2.Configuration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ray2.EventSources
{
    public interface IEventTraceability<TState, TStateKey> where TState : IState<TStateKey>, new()
    {
        TState TraceAsync(TState state, IEvent<TStateKey> @event);
        TState TraceAsync(TState state, IList<IEvent<TStateKey>> @events);
        Task<TState> TraceAsync(IEventSourcing<TState, TStateKey> eventSourcing);
        void Injection(TStateKey stateKey,StateSnapshotConfig config);
        Task<TState> ReadSnapshotAsync();
        Task SaveSnapshotAsync(TState state);
        Task ClearSnapshotAsync();
    }
}
