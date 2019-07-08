// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AzureStorage;
using MarginTrading.CommissionService.AzureRepositories.Entities;
using MarginTrading.CommissionService.Core.Domain.Abstractions;
using MarginTrading.CommissionService.Core.Repositories;

namespace MarginTrading.CommissionService.AzureRepositories.Repositories
{
    public class DailyPnlHistoryRepository : IDailyPnlHistoryRepository
    {
        private readonly INoSQLTableStorage<DailyPnlEntity> _tableStorage;
		
        public DailyPnlHistoryRepository(INoSQLTableStorage<DailyPnlEntity> tableStorage)
        {
            _tableStorage = tableStorage;
        }
        
        public Task BulkInsertAsync(List<IDailyPnlCalculation> calculations)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<IDailyPnlCalculation>> GetAsync(DateTime? @from, DateTime? to)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<IDailyPnlCalculation>> GetAsync(string accountId, DateTime? @from, DateTime? to)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(IDailyPnlCalculation obj)
        {
            throw new NotImplementedException();
        }

        public Task<int> SetWasCharged(string positionOperationId, bool type)
        {
            throw new NotImplementedException();
        }

        public Task<(int Total, int Failed, int NotProcessed)> GetOperationState(string operationId)
        {
            throw new NotImplementedException();
        }
    }
}