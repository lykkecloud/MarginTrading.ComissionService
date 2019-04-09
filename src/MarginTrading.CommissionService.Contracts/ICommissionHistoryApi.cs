using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.MarginTrading.CommissionService.Contracts.Models;
using Refit;

namespace Lykke.MarginTrading.CommissionService.Contracts
{
    /// <summary>
    /// Manages commission history.
    /// </summary>
    [PublicAPI]
    public interface ICommissionHistoryApi
    {   
        /// <summary>
        /// Retrieve overnight swap calculation history from storage between selected dates.
        /// </summary>
        [Get("/api/commission/overnight-swap")]
        Task<List<OvernightSwapHistoryContract>> GetOvernightSwapHistoryV2(
            [Query] DateTime from, [Query] DateTime to);
        
        /// <summary>
        /// Retrieve daily pnl calculation history from storage between selected dates.
        /// </summary>
        [Get("/api/commission/daily-pnl")]
        Task<List<DailyPnlHistoryContract>> GetDailyPnlHistory(
            [Query] DateTime from, [Query] DateTime to);
    }
}