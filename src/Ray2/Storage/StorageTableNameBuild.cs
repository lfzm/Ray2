using System;
using System.Collections.Generic;
using System.Text;

namespace Ray2.Storage
{
    public static class StorageTableNameBuild
    {
        /// <summary>
        /// Building snapshot event storage table name
        /// </summary>
        /// <param name="name">storage table name</param>
        /// <param name="type">storage type</param>
        /// <returns></returns>
        public static string BuildTableName(string name, StorageType type)
        {
            switch (type)
            {
                case StorageType.EventSource:
                    return $"ES_{name}";
                case StorageType.EventSourceSnapshot:
                    return $"ESS_{name}";
                case StorageType.EventProcessState:
                    return $"EP_{name}";
                default:
                    return "";
            }
           
        }

     
    }
}
