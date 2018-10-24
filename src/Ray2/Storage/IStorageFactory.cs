using Ray2.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Ray2.Storage
{
    public interface IStorageFactory
    {
        Task<IEventStorage> GetEventStorage(string name,  string stateKey);
        Task<List<EventStorageInfo>> GetEventStorageList(string name, long? createTime);
        Task<IStateStorage> GetStateStorage(string name, StorageType storageType, string stateKey);
        Task<string> GetTable(string name, StorageType storageType, string stateKey);
        Task<List<string>> GetTableList(string name, StorageType storageType, string stateKey, long? createTime);
    }
}
