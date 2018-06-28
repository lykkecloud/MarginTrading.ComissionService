using System;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using MarginTrading.CommissionService.Core.Domain.Abstractions;
using MarginTrading.CommissionService.Core.Repositories;

namespace MarginTrading.CommissionService.SqlRepositories.Repositories
{
    public class SqlLogRepository : ILogRepository
    {
        private readonly string _tableName;
        private const string CreateTableScript = "CREATE TABLE [{0}](" +
                                                 "[Id] [bigint] NOT NULL IDENTITY(1,1) PRIMARY KEY," +
                                                 "[DateTime] [DateTime] NULL," +
                                                 "[Level] [nvarchar] (64) NULL, " +
                                                 "[Env] [nvarchar] (64) NULL, " +
                                                 "[AppName] [nvarchar] (256) NULL, " +
                                                 "[Version] [nvarchar] (256) NULL, " +
                                                 "[Component] [nvarchar] (MAX) NULL, " +
                                                 "[Process] [nvarchar] (MAX) NULL, " +
                                                 "[Context] [nvarchar] (MAX) NULL, " +
                                                 "[Type] [nvarchar] (MAX) NULL, " +
                                                 "[Stack] [nvarchar] (MAX) NULL, " +
                                                 "[Msg] [nvarchar] (MAX) NULL " +
                                                 ");";
        
        private static Type DataType => typeof(ILogEntity);
        private static readonly string GetColumns = string.Join(",", DataType.GetProperties().Select(x => x.Name));
        private static readonly string GetFields = string.Join(",", DataType.GetProperties().Select(x => "@" + x.Name));
        private static readonly string GetUpdateClause = string.Join(",",
            DataType.GetProperties().Select(x => "[" + x.Name + "]=@" + x.Name));

        private readonly string _connectionString;
        
        public SqlLogRepository(string logTableName, string connectionString)
        {
            _tableName = logTableName;
            _connectionString = connectionString;
            
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.CreateTableIfDoesntExists(CreateTableScript, _tableName);
            }
        }
        
        public async Task Insert(ILogEntity log)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.ExecuteAsync(
                    $"insert into {_tableName} ({GetColumns}) values ({GetFields})", log);
            }
        }
    }
}