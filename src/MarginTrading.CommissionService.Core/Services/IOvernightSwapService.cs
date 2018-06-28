using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
        /// <returns></returns>
        Task<IReadOnlyList<IOvernightSwapCalculation>> Calculate(string operationId, DateTime creationTimestamp);
    }
}