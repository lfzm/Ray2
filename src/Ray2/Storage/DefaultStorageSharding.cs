
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Ray2.Storage
{
    public class DefaultStorageSharding : IStorageSharding
    {
        public Task<string> GetProvider(string name, StorageType type, string  stateKey)
        {
            return Task.FromResult(name);
        }

        public Task<List<string>> GetProviderList(string name, StorageType type)
        {
            return Task.FromResult(new List<string>() { name });
        }

        public Task<string> GetTable(string name, StorageType type, string stateKey)
        {
            return Task.FromResult(name);
        }

        public Task<List<string>> GetTableList(string name, StorageType type, string stateKey, long? createTime)
        {
            return Task.FromResult(new List<string>() { name });
        }
    }
}
