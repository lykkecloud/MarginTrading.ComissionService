// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using MarginTrading.Backend.Contracts.Positions;
using MarginTrading.CommissionService.Core.Repositories;
using MarginTrading.CommissionService.Core.Settings;
using Newtonsoft.Json;

namespace MarginTrading.CommissionService.SqlRepositories.Repositories
{
    public class TradingEngineSnapshotRepository : ITradingEngineSnapshotRepository
    {
        private readonly string _connectionString;

        public TradingEngineSnapshotRepository(CommissionServiceSettings settings)
        {
            _connectionString = settings.Db.StateConnString;
        }

        public async Task<List<OpenPositionContract>> GetPositionsAsync(DateTime tradingDay)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                var positionsStr = await conn.QueryFirstOrDefaultAsync<string>(
                    "SELECT TOP(1) Positions FROM dbo.TradingEngineSnapshots WHERE TradingDay=CAST(@TradingDay as date) ORDER BY Timestamp DESC",
                    new {TradingDay = tradingDay});

                return string.IsNullOrEmpty(positionsStr)
                    ? null
                    : JsonConvert.DeserializeObject<List<OpenPositionContract>>(positionsStr);
            }
        }
    }
}