using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Ray2.Storage
{
    public interface ISnapshotStorage: IStorage
    {
        Task CreateTable(string snapshotName);
        Task<TState> ReadAsync<TState>(string snapshotName, object stateId) where TState : IState, new();
        Task<bool> InsertAsync<TState>(string snapshotName, object stateId, TState state) where TState : IState, new();
        Task<bool> UpdateAsync<TState>(string snapshotName, object stateId, TState state) where TState : IState, new();
        Task<bool> DeleteAsync(string snapshotName, object stateId);
    }
}
