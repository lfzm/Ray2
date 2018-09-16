using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Ray2.PostgreSQL
{
    public interface IPostgreSqlDbContext : IDisposable
    {
        IDbTransaction BeginTransaction();
        Task OpenAsync();
        NpgsqlBinaryExporter BeginBinaryExport(string copyToCommand);
        NpgsqlBinaryImporter BeginBinaryImport(string copyFromCommand);
        Task<int> ExecuteAsync(string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null);
        Task<T> ExecuteScalarAsync<T>(string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null);
        Task<IEnumerable<T>> QueryAsync<T>(string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null);
    }
}
