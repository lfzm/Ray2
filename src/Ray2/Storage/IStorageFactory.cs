using Ray2.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Ray2.Storage
{
    public interface IStorageFactory
    {
        Task<IEventStorage> GetEventStorage(string eventSourceName, string stateKey);
        Task<string> GetEventTable(string eventSourceName, string stateKey);
        Task<List<string>> GetEventTableList(string eventSourceName, string stateKey, long? createTime);
        Task<List<EventStorageInfo>> GetEventStorageList(string eventSourceName, long? createTime);

        Task<IStateStorage> GetSnapshotStorage(string eventSourceName, string stateKey);
        Task<string> GetSnapshotTable(string eventSourceName, string stateKey);

        Task<IStateStorage> GetStateStorage(string eventProcessorName, string stateKey);
        Task<string> GetStateTable(string eventProcessorName, string stateKey);
    }
}
