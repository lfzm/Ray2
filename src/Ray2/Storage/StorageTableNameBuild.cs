using System;
using System.Collections.Generic;
using System.Text;

namespace Ray2.Storage
{
    public static class StorageTableNameBuild
    {
        /// <summary>
        /// Building event storage table name
        /// </summary>
        /// <param name="sourcingName">Event source name</param>
        /// <returns></returns>
        public static string BuildEventTableName(string sourcingName)
        {
            return $"ES_{sourcingName}";
        }

        /// <summary>
        /// Building snapshot event storage table name
        /// </summary>
        /// <param name="sourcingName">Event source name</param>
        /// <returns></returns>
        public static string BuildSnapshotTableName(string sourcingName)
        {
            return $"ESS_{sourcingName}";
        }

        /// <summary>
        /// Building event processing state storage table name
        /// </summary>
        /// <param name="processorName">Event processing name</param>
        /// <returns></returns>
        public static string BuildStatusTableName(string processorName)
        {
            return $"EP_{processorName}";
        }
    }
}
