using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Ray2.Storage
{
    public interface IStorageSharding
    {
        Task<string> GetProvider<TStateKey>(string name, StorageType type, TStateKey stateKey);
        Task<List<string>> GetProviderList(string name, StorageType type);
        Task<string> GetTable<TStateKey>(string tableName, StorageType type, TStateKey stateKey);
        Task<List<string>> GetTableList<TStateKey>(string tableName, StorageType type, TStateKey stateKey, long? createTime);
    }
}
