// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Common.Log;
using Dapper;
using MarginTrading.CommissionService.Core.Domain;
using MarginTrading.CommissionService.Core.Domain.Abstractions;
using MarginTrading.CommissionService.Core.Repositories;
using MarginTrading.CommissionService.Core.Services;
using MarginTrading.CommissionService.Core.Settings;
using MarginTrading.CommissionService.SqlRepositories.Entities;

namespace MarginTrading.CommissionService.SqlRepositories.Repositories
{
    public class CostsAndChargesRepository : ICostsAndChargesRepository
    {
        private const int BulkPageSize = 10000;
        private const string TableName = "CostsAndChangesCalculations";
        private const string CreateTableScript = @"CREATE TABLE [{0}](
  [Id] [nvarchar] (128) NOT NULL PRIMARY KEY,
[AccountId] [nvarchar] (64) NOT NULL,
[Instrument] [nvarchar] (64) NOT NULL,
[TimeStamp] [DateTime] NOT NULL,
[Volume] float NOT NULL,
[Direction] [nvarchar] (64) NOT NULL,
[Data] [nvarchar] (MAX) NULL
INDEX IX_CostsAndChanges NONCLUSTERED (AccountId, Instrument, TimeStamp, Volume, Direction)
);";
        
        private static Type DataType => typeof(CostsAndChargesEntity);
        private static readonly string GetColumns = string.Join(",", DataType.GetProperties().Select(x => x.Name));
        private static readonly string GetFields = string.Join(",", DataType.GetProperties().Select(x => "@" + x.Name));
        
        private static readonly string SharedCalculationsColumns = GetColumns.Replace(nameof(CostsAndChargesEntity.AccountId),
            $"'' as {nameof(CostsAndChargesEntity.AccountId)}");
        
        private ILog _log;
        private CommissionServiceSettings _settings;
        private readonly IAccountRedisCache _accountRedisCache;

        public CostsAndChargesRepository(
            CommissionServiceSettings settings,
            IAccountRedisCache accountRedisCache,
            ILog log)
        {
            _log = log;
            _settings = settings;
            _accountRedisCache = accountRedisCache;

            using (var conn = new SqlConnection(_settings.Db.StateConnString))
            {
                try
                {
                    conn.CreateTableIfDoesntExists(CreateTableScript, TableName);
                }
                catch (Exception ex)
                {
                    _log?.WriteErrorAsync(nameof(CostsAndChargesRepository), "CreateTableIfDoesntExists", null, ex);
                    throw;
                }
            }
        }
        

        public async Task Save(CostsAndChargesCalculation calculation)
        {
            using (var conn = new SqlConnection(_settings.Db.StateConnString))
            {
                await conn.ExecuteAsync(
                    $"insert into {TableName} ({GetColumns}) values ({GetFields})",
                    Map(calculation));
            }
        }

        public async Task<PaginatedResponse<CostsAndChargesCalculation>> Get(string accountId, string instrument, 
        decimal? quantity, OrderDirection? direction, DateTime? @from, DateTime? to, int? skip,
            int? take, bool isAscendingOrder = true)
        {
            take = PaginationHelper.GetTake(take);

            string sharedFilter = "", baseAssetId = "", tradingConditionId = "", legalEntity = "";

            if (!string.IsNullOrEmpty(accountId))
            {
                var account = await _accountRedisCache.GetAccount(accountId);

                if (account == null)
                {
                    throw new Exception($"Account with ID {accountId} does not exist");
                }

                sharedFilter = "WHERE BaseAssetId = @baseAssetId " +
                               "AND TradingConditionId = @tradingConditionId " +
                               "AND LegalEntity = @legalEntity";

                baseAssetId = account.BaseAssetId;
                tradingConditionId = account.TradingConditionId;
                legalEntity = account.LegalEntity;
            }
            
            var union = $"(SELECT {GetColumns} FROM {TableName} UNION " +
                        $"SELECT {SharedCalculationsColumns} FROM {SharedCostsAndChargesRepository.TableName} {sharedFilter}) as tmp";

            var whereClause = "WHERE 1=1 "
                              + (string.IsNullOrWhiteSpace(accountId) ? "" : " AND (AccountId = @accountId OR AccountId = '')")
                              + (string.IsNullOrWhiteSpace(instrument) ? "" : " AND Instrument = @instrument")
                              + (!quantity.HasValue ? "" : " AND Volume = @quantity")
                              + (!direction.HasValue ? "" : " AND Direction = @direction")
                              + (from == null ? "" : " AND TimeStamp >= @from")
                              + (to == null ? "" : " AND TimeStamp < @to");
            
            var sorting = isAscendingOrder ? "ASC" : "DESC";
            var paginationClause = $" ORDER BY [TimeStamp] {sorting} OFFSET {skip ?? 0} ROWS FETCH NEXT {take} ROWS ONLY";
            
            using (var conn = new SqlConnection(_settings.Db.StateConnString))
            {
                var sql =
                    $"SELECT * FROM {union} {whereClause} {paginationClause}; SELECT COUNT(*) FROM {union} {whereClause}";
                var gridReader = await conn.QueryMultipleAsync(sql,
                    new
                    {
                        accountId,
                        instrument,
                        baseAssetId,
                        tradingConditionId,
                        legalEntity,
                        quantity,
                        direction = direction.ToString(),
                        from,
                        to
                    });

                var contents = (await gridReader.ReadAsync<CostsAndChargesEntity>()).ToList();
                var totalCount = await gridReader.ReadSingleAsync<int>();

                return new PaginatedResponse<CostsAndChargesCalculation>(
                    contents: contents.Select(Map).ToArray(), 
                    start: skip ?? 0, 
                    size: contents.Count, 
                    totalSize: totalCount
                );
            }
        }

        public async Task<CostsAndChargesCalculation[]> GetByIds(string accountId, string[] ids)
        {
            using (var conn = new SqlConnection(_settings.Db.StateConnString))
            {
                var entities = await conn.QueryAsync<CostsAndChargesEntity>(
                    $"SELECT * FROM {TableName} WHERE [AccountId] = @accountId AND Id IN @ids", 
                    new { accountId, ids });
                
                var result = entities.Select(Map).ToList();

                if (result.Count < ids?.Length)
                {
                    var sharedEntities = await conn.QueryAsync<CostsAndChargesEntity>(
                        $"SELECT * FROM {SharedCostsAndChargesRepository.TableName} WHERE Id IN @ids", 
                        new { ids });

                    result.AddRange(sharedEntities.Select(Map));
                }

                return result.ToArray();
            }
        }

        public async Task<PaginatedResponse<CostsAndChargesCalculation>> GetAllByDay(DateTime date, int? skip, int? take)
        {
            using (var conn = new SqlConnection(_settings.Db.StateConnString))
            {
                var union = $"(SELECT {GetColumns} FROM {TableName} UNION " +
                            $"SELECT {SharedCalculationsColumns} FROM {SharedCostsAndChargesRepository.TableName}) as tmp";
                var whereClause = "WHERE TimeStamp >= @day AND TimeStamp < @nextDay";
                var paginationClause = $"ORDER BY [TimeStamp] OFFSET {skip ?? 0} ROWS FETCH NEXT {take ?? BulkPageSize} ROWS ONLY";

                var reader = await conn.QueryMultipleAsync(
                    $"SELECT * FROM {union} {whereClause} {paginationClause}; SELECT COUNT(*) FROM {union} {whereClause}",
                    new
                    {
                        day = date.Date,
                        nextDay = date.Date.AddDays(1)
                    });

                var contents = (await reader.ReadAsync<CostsAndChargesEntity>()).ToList();
                var totalCount = await reader.ReadSingleAsync<int>();

                return new PaginatedResponse<CostsAndChargesCalculation>(
                    contents: contents.Select(Map).ToArray(),
                    start: skip ?? 0,
                    size: contents.Count,
                    totalSize: totalCount
                );
            }
        }

        private CostsAndChargesEntity Map(CostsAndChargesCalculation calculation)
        {
            string data;
            var serializer = new XmlSerializer(typeof(CostsAndChargesCalculation));

            using (var writer = new StringWriter())
            {
                serializer.Serialize(writer, calculation);
                data = writer.ToString();
            }

            var signedData = RsaHelper.SignData(data, _settings.SignatureSettings);

            return new CostsAndChargesEntity
            {
                Id = calculation.Id,
                AccountId = calculation.AccountId,
                Instrument = calculation.Instrument,
                Volume = calculation.Volume,
                Direction = calculation.Direction.ToString(),
                Timestamp = calculation.Timestamp,
                Data = signedData
            };
        }

        private CostsAndChargesCalculation Map(CostsAndChargesEntity entity)
        {
            if (!RsaHelper.ValidateSign(entity.Data, _settings.SignatureSettings, out var error))
            {
                throw new Exception($"Document with id {entity.Id} has invalid signature. {error}");
            }
            
            var serializer = new XmlSerializer(typeof(CostsAndChargesCalculation));
            
            using (var reader = new StringReader(entity.Data))
            {
                return serializer.Deserialize(reader) as CostsAndChargesCalculation;
            }
        }
    }
}