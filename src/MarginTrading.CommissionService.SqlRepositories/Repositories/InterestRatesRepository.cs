// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Common.Log;
using Dapper;
using MarginTrading.CommissionService.Core.Domain.Abstractions;
using MarginTrading.CommissionService.Core.Repositories;
using MarginTrading.CommissionService.Core.Services;
using MarginTrading.CommissionService.Core.Settings;
using MarginTrading.CommissionService.SqlRepositories.Entities;
using Microsoft.Extensions.Internal;

namespace MarginTrading.CommissionService.SqlRepositories.Repositories
{
    public class InterestRatesRepository : IInterestRatesRepository
    {
        private const string TableName = "[bookkeeper].[ClosingInterestRate]";
        
        private static Type DataType => typeof(IInterestRate);
        private static readonly string GetColumns = string.Join(",", DataType.GetProperties().Select(x => x.Name));
        private static readonly string GetFields = string.Join(",", DataType.GetProperties().Select(x => "@" + x.Name));
        private static readonly string GetUpdateClause = string.Join(",",
            DataType.GetProperties().Select(x => "[" + x.Name + "]=@" + x.Name));

        private readonly IConvertService _convertService;
        private readonly ISystemClock _systemClock;
        private readonly CommissionServiceSettings _settings;
        private readonly ILog _log;
        
        public InterestRatesRepository(IConvertService convertService, ISystemClock systemClock, 
            CommissionServiceSettings settings, ILog log)
        {
            _convertService = convertService;
            _systemClock = systemClock;
            _log = log;
            _settings = settings;
            
            using (var conn = new SqlConnection(_settings.Db.StateConnString))
            {
                try { conn.CheckIfTableExists(TableName); }
                catch (Exception ex)
                {
                    _log?.WriteErrorAsync(nameof(InterestRatesRepository), "CheckIfTableExists", null, ex);
                    throw;
                }
            }
        }
        
        public async Task<IReadOnlyList<IInterestRate>> GetAll()
        {
            using (var conn = new SqlConnection(_settings.Db.StateConnString))
            {
                var data = await conn.QueryAsync<InterestRate>($"SELECT * FROM {TableName}");
                
                return data.ToList();
            }
        }

        public async Task<IReadOnlyList<IInterestRate>> GetAllLatest()
        {
            using (var conn = new SqlConnection(_settings.Db.StateConnString))
            {
                var data = await conn.QueryAsync<InterestRate>(
                    string.Format(@";with cteRowNumber as (select MdsCode, ClosePrice, Timestamp, row_number() 
 over(partition by MdsCode order by Timestamp desc) as RowNum
 from {0}
 )
 select MdsCode, ClosePrice, Timestamp
 from cteRowNumber
 where RowNum = 1", TableName));
                
                return data.ToList();
            }
        }
    }
}