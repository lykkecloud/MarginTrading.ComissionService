using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MarginTrading.CommissionService.Core.Domain.Abstractions;

namespace MarginTrading.CommissionService.Core.Repositories
{
    public interface IOvernightSwapHistoryRepository
    {
        Task AddAsync(IOvernightSwap obj);
        Task<IEnumerable<IOvernightSwap>> GetAsync();
        Task<IReadOnlyList<IOvernightSwap>> GetAsync(DateTime? @from, DateTime? to);
        Task<IReadOnlyList<IOvernightSwap>> GetAsync(string accountId, DateTime? from, DateTime? to);

        /// <summary>
        /// For testing purposes
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        Task DeleteAsync(IOvernightSwap obj);
    }
}