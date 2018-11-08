using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Ray2.PostgreSQL
{
   public interface IPostgreSqlStateStorage
    {
        Task<bool> DeleteAsync(object stateId);
        Task<bool> InsertAsync<TState>(object stateId, TState state) where TState : IState, new();
        Task<TState> ReadAsync<TState>(object stateId) where TState : IState, new();
        Task<bool> UpdateAsync<TState>(object stateId, TState state) where TState : IState, new();
    }
}
