using Dapper;
using Npgsql;
using Ray2.PostgreSQL.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using System.Threading.Tasks;

namespace Ray2.PostgreSQL
{
    public class PostgreSqlDbContext : IPostgreSqlDbContext
    {
        private readonly NpgsqlConnection dbConnection;
        private readonly PostgreSqlOptions options;
        public PostgreSqlDbContext(PostgreSqlOptions options)
        {
            this.options = options;
            dbConnection = new NpgsqlConnection();
            dbConnection.ConnectionString = this.options.ConnectionString;
        }

        public IDbTransaction BeginTransaction()
        {
            return this.dbConnection.BeginTransaction();
        }
        public IDbTransaction BeginTransaction(IsolationLevel il)
        {
            return this.dbConnection.BeginTransaction(il);
        }
        public NpgsqlBinaryExporter BeginBinaryExport(string copyToCommand)
        {
            return this.dbConnection.BeginBinaryExport(copyToCommand);
        }
        public NpgsqlBinaryImporter BeginBinaryImport(string copyFromCommand)
        {
            return this.dbConnection.BeginBinaryImport(copyFromCommand);
        }
        public void Dispose()
        {
            this.dbConnection.Dispose();
        }
        public Task OpenAsync()
        {
            this.dbConnection.Open();
            return Task.CompletedTask;
        }
        public Task<int> ExecuteAsync(string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return this.dbConnection.ExecuteAsync(sql, param, transaction, commandTimeout, commandType);
        }
        public Task<T> ExecuteScalarAsync<T>(string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return this.dbConnection.ExecuteScalarAsync<T>(sql, param, transaction, commandTimeout, commandType);
        }
        public Task<IEnumerable<T>> QueryAsync<T>(string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return this.dbConnection.QueryAsync<T>(sql, param, transaction, commandTimeout, commandType);
        }
    }
}
