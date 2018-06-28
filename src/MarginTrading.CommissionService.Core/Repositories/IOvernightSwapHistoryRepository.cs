using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MarginTrading.CommissionService.Core.Domain.Abstractions;

namespace MarginTrading.CommissionService.Core.Repositories
{
    public interface IOvernightSwapHistoryRepository
    {
        Task AddAsync(IOvernightSwapCalculation obj);
        Task<IEnumerable<IOvernightSwapCalculation>> GetAsync();
        Task<IReadOnlyList<IOvernightSwapCalculation>> GetAsync(DateTime? @from, DateTime? to);
        Task<IReadOnlyList<IOvernightSwapCalculation>> GetAsync(string accountId, DateTime? from, DateTime? to);

        /// <summary>
        /// For testing purposes
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        Task DeleteAsync(IOvernightSwapCalculation obj);
    }
}