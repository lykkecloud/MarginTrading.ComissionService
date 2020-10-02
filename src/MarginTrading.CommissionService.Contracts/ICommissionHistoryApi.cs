// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MarginTrading.CommissionService.Contracts.Models;
using Refit;

namespace MarginTrading.CommissionService.Contracts
{
    /// <summary>
    /// Manages commission history.
    /// </summary>
    [PublicAPI]
    public interface ICommissionHistoryApi
    {   
        /// <summary>
        /// Retrieve overnight swap calculation history from storage between selected trading days.
        /// </summary>
        [Get("/api/commission/overnight-swap")]
        Task<List<OvernightSwapHistoryContract>> GetOvernightSwapHistory(
            [Query] DateTime from, [Query] DateTime to, [Query] string accountId);
        
        /// <summary>
        /// Retrieve daily pnl calculation history from storage between selected trading days.
        /// </summary>
        [Get("/api/commission/daily-pnl")]
        Task<List<DailyPnlHistoryContract>> GetDailyPnlHistory(
            [Query] DateTime from, [Query] DateTime to, [Query] string accountId);
    }
}