using Ray2.Configuration;
using Ray2.Storage;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ray2.EventSource
{
    /// <summary>
    /// This is the event source interface
    /// </summary>
    public interface IEventSourcing<TState, TStateKey>
        where TState : IState<TStateKey>, new()
    {

        Task<IEventSourcing<TState, TStateKey>> Init(TStateKey stateKey);
        Task<bool> SaveAsync(IEvent<TStateKey> @event);
        Task<bool> SaveAsync(IList<IEvent<TStateKey>> events);
        Task<List<IEvent<TStateKey>>> GetListAsync(EventQueryModel queryModel);

        Task<TState> ReadSnapshotAsync();
        Task SaveSnapshotAsync(TState state);
        Task ClearSnapshotAsync();

        TState TraceAsync(TState state, IEvent<TStateKey> @event);
        TState TraceAsync(TState state, IList<IEvent<TStateKey>> @events);
    }

}
