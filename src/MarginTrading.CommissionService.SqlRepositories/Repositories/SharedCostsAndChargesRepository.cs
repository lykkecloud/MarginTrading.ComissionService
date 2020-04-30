// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Common.Log;
using Dapper;
using MarginTrading.CommissionService.Core.Domain;
using MarginTrading.CommissionService.Core.Repositories;
using MarginTrading.CommissionService.Core.Settings;
using MarginTrading.CommissionService.SqlRepositories.Entities;

namespace MarginTrading.CommissionService.SqlRepositories.Repositories
{
    public class SharedCostsAndChargesRepository : ISharedCostsAndChargesRepository
    {
        public const string TableName = "SharedCostsAndChangesCalculations";

        private const string CreateTableScript = @"CREATE TABLE [{0}](
  [Id] [nvarchar] (128) NOT NULL PRIMARY KEY,
[Instrument] [nvarchar] (64) NOT NULL,
[BaseAssetId] [nvarchar] (64) NOT NULL,
[TradingConditionId] [nvarchar] (64) NOT NULL,
[LegalEntity] [nvarchar] (64) NOT NULL,
[TimeStamp] [DateTime] NOT NULL,
[Volume] float NOT NULL,
[Direction] [nvarchar] (64) NOT NULL,
[Data] [nvarchar] (MAX) NULL
INDEX IX_SharedCostsAndChanges NONCLUSTERED (Instrument, TimeStamp, BaseAssetId, TradingConditionId, LegalEntity)
);";
        
        private readonly ILog _log;
        
        private readonly CommissionServiceSettings _settings;
        
        private static Type DataType => typeof(SharedCostsAndChargesEntity);
        
        private static readonly string GetColumns = string.Join(",", DataType.GetProperties().Select(x => x.Name));
        
        private static readonly string GetFields = string.Join(",", DataType.GetProperties().Select(x => "@" + x.Name));
        
        public SharedCostsAndChargesRepository(
            CommissionServiceSettings settings,
            ILog log)
        {
            _settings = settings;
            _log = log;

            using (var conn = new SqlConnection(_settings.Db.StateConnString))
            {
                try
                {
                    conn.CreateTableIfDoesntExists(CreateTableScript, TableName);
                }
                catch (Exception ex)
                {
                    _log?.WriteErrorAsync(nameof(SharedCostsAndChargesRepository), "CreateTableIfDoesntExists", null, ex);
                    throw;
                }
            }
        }
        
        public async Task SaveAsync(CostsAndChargesCalculation calculation)
        {
            using (var conn = new SqlConnection(_settings.Db.StateConnString))
            {
                await conn.ExecuteAsync(
                    $"insert into {TableName} ({GetColumns}) values ({GetFields})",
                    Map(calculation));
            }
        }

        public async Task<List<string>> GetAssetPairIdsWithFilesAsync(DateTime date)
        {
            using (var conn = new SqlConnection(_settings.Db.StateConnString))
            {
                var result = await conn.QueryAsync<string>(
                    $"SELECT DISTINCT {nameof(CostsAndChargesCalculation.Instrument)} FROM {TableName} " +
                    $"WHERE DATEDIFF(DAY, {nameof(CostsAndChargesCalculation.Timestamp)}, @date) = 0",
                    new {date.Date});
                
                return result.ToList();
            }
        }
        
        private SharedCostsAndChargesEntity Map(CostsAndChargesCalculation calculation)
        {
            string data;
            var serializer = new XmlSerializer(typeof(CostsAndChargesCalculation));

            using (var writer = new StringWriter())
            {
                serializer.Serialize(writer, calculation);
                data = writer.ToString();
            }

            var signedData = RsaHelper.SignData(data, _settings.SignatureSettings);

            return new SharedCostsAndChargesEntity
            {
                Id = calculation.Id,
                Instrument = calculation.Instrument,
                LegalEntity = calculation.LegalEntity,
                TradingConditionId = calculation.TradingConditionId,
                BaseAssetId = calculation.BaseAssetId,
                Volume = calculation.Volume,
                Direction = calculation.Direction.ToString(),
                Timestamp = calculation.Timestamp,
                Data = signedData
            };
        }
    }
}