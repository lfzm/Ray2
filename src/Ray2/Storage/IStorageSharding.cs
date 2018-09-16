using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Ray2.Storage
{
    public interface IStorageSharding
    {
        /// <summary>
        /// Get the Storage Provider Name based on the state key
        /// </summary>
        /// <typeparam name="TStateKey"></typeparam>
        /// <param name="stateKey">state key</param>
        /// <returns></returns>
        string GetProvider<TStateKey>(TStateKey stateKey);
        /// <summary>
        ///Get all the storage providers
        /// </summary>
        /// <returns></returns>
        List<string> GetProviderList();

        Task<string> GetTable<TStateKey>(TStateKey stateKey);
        Task<List<string>> GetTableList<TStateKey>(TStateKey stateKey, long? createTime);
    }
}
