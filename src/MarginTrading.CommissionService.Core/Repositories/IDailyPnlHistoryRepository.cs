// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MarginTrading.CommissionService.Core.Domain.Abstractions;

namespace MarginTrading.CommissionService.Core.Repositories
{
    public interface IDailyPnlHistoryRepository
    {
        Task BulkInsertAsync(List<IDailyPnlCalculation> calculations);
        
        Task<IReadOnlyList<IDailyPnlCalculation>> GetAsync(DateTime? @from, DateTime? to);
        Task<IReadOnlyList<IDailyPnlCalculation>> GetAsync(string accountId, DateTime? from, DateTime? to);

        /// <summary>
        /// For testing purposes
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        Task DeleteAsync(IDailyPnlCalculation obj);

        Task<int> SetWasCharged(string positionOperationId, bool type);
        
        Task<(int Total, int Failed, int NotProcessed)> GetOperationState(string operationId);
    }
}