
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Ray2.Storage
{
    public class DefaultStorageSharding : IStorageSharding
    {
        public Task<string> GetProvider<TStateKey>(string name, StorageType type, TStateKey stateKey)
        {
            return Task.FromResult(name);
        }

        public Task<List<string>> GetProviderList(string name, StorageType type)
        {
            return Task.FromResult(new List<string>() { name });
        }

        public Task<string> GetTable<TStateKey>(string tableName, StorageType type, TStateKey stateKey)
        {
            return Task.FromResult(tableName);
        }

        public Task<List<string>> GetTableList<TStateKey>(string tableName, StorageType type, TStateKey stateKey, long? createTime)
        {
            return Task.FromResult(new List<string>() { tableName });
        }
    }
}
