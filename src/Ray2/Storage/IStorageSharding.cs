using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Ray2.Storage
{
    public interface IStorageSharding
    {
        Task<string> GetProvider(string name, StorageType type, string stateKey);
        Task<List<string>> GetProviderList(string name, StorageType type);
        Task<string> GetTable(string name, StorageType type, string stateKey);
        Task<List<string>> GetTableList(string name, StorageType type, string stateKey, long? createTime);
    }
}
