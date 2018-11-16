using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.MarginTrading.CommissionService.Contracts.Models;
using Refit;

namespace Lykke.MarginTrading.CommissionService.Contracts
{
    /// <summary>
    /// Manages overnight swaps.
    /// </summary>
    [PublicAPI]
    public interface ICommissionHistoryApi
    {
        /// <summary>
        /// Retrieve overnight swap calculation history from storage between selected dates.
        /// </summary>
        [Get("/api/overnightswap/history")]
        Task<List<OvernightSwapHistoryContract>> GetOvernightSwapHistory(
            [Query] DateTime from, [Query] DateTime to);
    }
}