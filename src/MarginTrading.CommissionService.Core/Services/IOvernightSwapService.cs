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
        /// <returns></returns>
        Task<IReadOnlyList<IOvernightSwapCalculation>> Calculate(string operationId, DateTime creationTimestamp,
            int numberOfFinancingDays, int financingDaysPerYear);

        /// <summary>
        /// True if operation with <param name="operationId"/> was never called before.
        /// </summary>
        /// <param name="operationId"></param>
        /// <returns></returns>
        Task<bool> CheckOperationIsNew(string operationId);
        Task<bool> CheckPositionOperationIsNew(string positionOperationId);
        Task SetWasCharged(string positionOperationId);
    }
}