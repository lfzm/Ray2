using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Ray2.PostgreSQL
{
    public interface IPostgreSqlTableStorage
    {
        Task CreateEventTable(TableInfo tableInfo);
        Task CreateSnapshotTable(string name);
        Task CreateShardingTable();
        Task<List<TableInfo>> GetEventTableList(string prefix);
        Task<long> GetEventTableCount(string name);
    }
}
