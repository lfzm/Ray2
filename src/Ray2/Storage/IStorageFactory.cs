using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ray2.Storage
{
    /// <summary>
    /// Repository factory
    /// </summary>
    public interface IStorageFactory
    {
        /// <summary>
        /// Obtain event storage based on repository name and status key
        /// </summary>
        /// <param name="name">storage name</param>
        /// <param name="stateKey">State Key</param>
        /// <returns></returns>
        Task<IEventStorage> GetEventStorage(string name,  string stateKey);
        /// <summary>
        /// Get all event storage in the repository based on the repository name and storage creation time
        /// </summary>
        /// <param name="name">storage name</param>
        /// <param name="createTime">storage creation time</param>
        /// <returns></returns>
        Task<List<EventStorageInfo>> GetEventStorageList(string name, long? createTime);
        /// <summary>
        /// Get state storage based on repository name and state key
        /// </summary>
        /// <param name="name">storage name</param>
        /// <param name="storageType">state storage Type </param>
        /// <param name="stateKey">State Key</param>
        /// <returns></returns>
        Task<IStateStorage> GetStateStorage(string name, StorageType storageType, string stateKey);
        /// <summary>
        /// Get the storage table name based on the repository name and state key
        /// </summary>
        /// <param name="name">storage name</param>
        /// <param name="storageType">state storage Type </param>
        /// <param name="stateKey">State Key</param>
        /// <returns></returns>
        Task<string> GetTable(string name, StorageType storageType, string stateKey);
        /// <summary>
        /// Get all storage table names based on repository name and state key
        /// </summary>
        /// <param name="name">storage name</param>
        /// <param name="storageType">state storage Type </param>
        /// <param name="stateKey">State Key</param>
        /// <param name="createTime">storage creation time</param>
        /// <returns></returns>
        Task<List<string>> GetTableList(string name, StorageType storageType, string stateKey, long? createTime);
    }
}
