using Ray2.Configuration;
using Ray2.Storage;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ray2.EventSources
{
    /// <summary>
    /// This is the event source interface
    /// </summary>
    public interface IEventSourcing<TState, TStateKey>
        where TState : IState<TStateKey>, new()
    {
        void Injection(TStateKey stateKey, EventSourcesConfig config);
        Task<bool> SaveAsync(IEvent<TStateKey> @event);
        Task<bool> SaveAsync(IList<IEvent<TStateKey>> events);
        Task<List<IEvent<TStateKey>>> GetListAsync(EventQueryModel queryModel);
    }
    /// <summary>
    /// This is the event source interface
    /// </summary>
    public interface IEventSourcing
    {

    }
}
