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
        Task<IList<IEventStorage>> GetEventStorageList(string eventSourceName);
        Task<StorageTableInfo> GetEventTable(string eventSourceName, string stateKey);
        Task<List<StorageTableInfo>> GetEventTableList(string eventSourceName, long? createTime);
        Task<List<StorageTableInfo>> GetEventTableList(string eventSourceName, string stateKey, long? createTime);


        Task<IStateStorage> GetSnapshotStorage(string eventSourceName, string stateKey);
        Task<StorageTableInfo> GetSnapshotTable(string eventSourceName, string stateKey);

        Task<IStateStorage> GetStateStorage(string eventProcessorName, string stateKey);
        Task<StorageTableInfo> GetStateTable(string eventProcessorName, string stateKey);
    }
}
