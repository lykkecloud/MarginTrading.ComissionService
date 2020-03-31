// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Common.Log;
using Dapper;
using MarginTrading.CommissionService.Core;
using MarginTrading.CommissionService.Core.Domain.Abstractions;
using MarginTrading.CommissionService.Core.Repositories;
using MarginTrading.CommissionService.Core.Services;
using MarginTrading.CommissionService.Core.Settings;
using MarginTrading.CommissionService.SqlRepositories.Entities;
using Microsoft.Extensions.Internal;

namespace MarginTrading.CommissionService.SqlRepositories.Repositories
{
    public class OvernightSwapHistoryRepository : IOvernightSwapHistoryRepository
    {
        private const string TableName = "OvernightSwapHistory";
        private const string CreateTableScript = "CREATE TABLE [{0}](" +
                                                 "[Id] [nvarchar] (128) NOT NULL PRIMARY KEY," +
                                                 "[OperationId] [nvarchar] (64) NOT NULL," +
                                                 "[AccountId] [nvarchar] (64) NOT NULL, " +
                                                 "[Instrument] [nvarchar] (64) NOT NULL, " +
                                                 "[Direction] [nvarchar] (64) NOT NULL, " +
                                                 "[Time] [DateTime] NOT NULL," +
                                                 "[Volume] float NOT NULL, " +
                                                 "[SwapValue] float NOT NULL, " +
                                                 "[PositionId] [nvarchar] (64) NOT NULL, " +
                                                 "[Details] [nvarchar] (MAX) NULL," +
                                                 "[IsSuccess] [bit] NOT NULL, " +
                                                 "[Exception] [nvarchar] (MAX) NULL," +
                                                 "[WasCharged] [bit] NULL," +
                                                 "[TradingDay] [DATETIME] NOT NULL," +
                                                 "INDEX IX_OSH NONCLUSTERED (Time, TradingDay, AccountId, OperationId, PositionId, WasCharged)" +
                                                 ");";
        
        private static Type DataType => typeof(IOvernightSwapCalculation);
        private static readonly string GetColumns = string.Join(",", DataType.GetProperties().Select(x => x.Name));
        private static readonly string GetFields = string.Join(",", DataType.GetProperties().Select(x => "@" + x.Name));
        private static readonly string GetUpdateClause = string.Join(",",
            DataType.GetProperties().Select(x => "[" + x.Name + "]=@" + x.Name));

        private readonly IConvertService _convertService;
        private readonly ISystemClock _systemClock;
        private readonly CommissionServiceSettings _settings;
        private readonly ILog _log;
        
        public OvernightSwapHistoryRepository(IConvertService convertService, ISystemClock systemClock, 
            CommissionServiceSettings settings, ILog log)
        {
            _convertService = convertService;
            _systemClock = systemClock;
            _log = log;
            _settings = settings;
            
            using (var conn = new SqlConnection(_settings.Db.StateConnString))
            {
                try { conn.CreateTableIfDoesntExists(CreateTableScript, TableName); }
                catch (Exception ex)
                {
                    _log?.WriteErrorAsync(nameof(OvernightSwapHistoryRepository), "CreateTableIfDoesntExists", null, ex);
                    throw;
                }
            }
        }

        public async Task BulkInsertAsync(List<IOvernightSwapCalculation> overnightSwapCalculations)
        {
            using (var conn = new SqlConnection(_settings.Db.StateConnString))
            {
                await conn.ExecuteAsync(
                    $"insert into {TableName} ({GetColumns}) values ({GetFields})", 
                    overnightSwapCalculations.Select(OvernightSwapEntity.Create));
            }
        }

        public async Task<IReadOnlyList<IOvernightSwapCalculation>> GetAsync(DateTime? @from, DateTime? to)
        {
            return (await GetFilteredAsync(null, from, to)).ToList();
        }

        public async Task<IReadOnlyList<IOvernightSwapCalculation>> GetAsync(string accountId, DateTime? @from, DateTime? to)
        {
            return (await GetFilteredAsync(accountId, from, to)).ToList();
        }

        public async Task DeleteAsync(IOvernightSwapCalculation obj)
        {
            using (var conn = new SqlConnection(_settings.Db.StateConnString))
            {
                await conn.ExecuteAsync(
                    $"DELETE {TableName} WHERE Id=@Id", new { Id = obj.OperationId});
            }
        }

        public async Task<int> SetWasCharged(string positionOperationId, bool type)
        {
            using (var conn = new SqlConnection(_settings.Db.StateConnString))
            {
                return await conn.ExecuteAsync(
                    $"UPDATE {TableName} SET [WasCharged]=@WasCharged WHERE Id=@Id", 
                    new
                    {
                        Id = positionOperationId,
                        WasCharged = type,
                    });
            }
        }

        public async Task<(int Total, int Failed, int NotProcessed)> GetOperationState(string operationId)
        {
            using (var conn = new SqlConnection(_settings.Db.StateConnString))
            {
                var (total, failed, notProcessed) = await conn.QuerySingleAsync<(int total, int failed, int notProcessed)>(
                    $@"SELECT
  (SELECT COUNT(*) FROM {TableName} WHERE OperationId=@OperationId AND IsSuccess=1) total,
  (SELECT COUNT(*) FROM {TableName} WHERE OperationId=@OperationId AND IsSuccess=1 AND WasCharged=0) failed,
  (SELECT COUNT(*) FROM {TableName} WHERE OperationId=@OperationId AND IsSuccess=1 AND WasCharged IS NULL) notProcessed", 
                    new { OperationId = operationId });

                return (total, failed, notProcessed);
            }
        }

        private async Task<IEnumerable<IOvernightSwapCalculation>> GetFilteredAsync(string accountId = null,
            DateTime? @from = null, DateTime? to = null)
        {
            using (var conn = new SqlConnection(_settings.Db.StateConnString))
            {
                var whereClause = "WHERE 1=1 " +
                                  (string.IsNullOrWhiteSpace(accountId) ? "" : " AND AccountId = @accountId")
                                  + (from == null ? "" : " AND TradingDay >= @from")
                                  + (to == null ? "" : " AND TradingDay < @to");
                var swapEntities = await conn.QueryAsync<OvernightSwapEntity>(
                    $"SELECT * FROM {TableName} {whereClause}", 
                    new { accountId, from, to });
                
                return swapEntities.ToList();
            }
        }
    }
}