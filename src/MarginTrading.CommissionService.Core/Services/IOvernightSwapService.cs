using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Cqrs;
using MarginTrading.CommissionService.Core.Domain.Abstractions;

namespace MarginTrading.CommissionService.Core.Services
{
    /// <summary>
    /// Take care of overnight swap calculation and charging.
    /// </summary>
    public interface IOvernightSwapService
    {
        /// <summary>
        /// Entry point for overnight swaps calculation. Successfully calculated swaps are returned.
        /// </summary>
        /// <param name="operationId"></param>
        /// <param name="creationTimestamp"></param>
        /// <param name="numberOfFinancingDays"></param>
        /// <param name="financingDaysPerYear"></param>
        /// <param name="tradingDay"></param>
        /// <returns></returns>
        Task<IReadOnlyList<IOvernightSwapCalculation>> Calculate(string operationId, DateTime creationTimestamp,
            int numberOfFinancingDays, int financingDaysPerYear, DateTime tradingDay);

        Task<int> SetWasCharged(string positionOperationId, bool type);
        
        Task<(int Total, int Failed, int NotProcessed)> GetOperationState(string id);
    }
}