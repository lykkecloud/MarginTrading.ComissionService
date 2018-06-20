using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MarginTrading.OvernightSwapService.Contracts.Models;
using Refit;

namespace MarginTrading.OvernightSwapService.Contracts
{
    /// <summary>
    /// Manages overnight swaps.
    /// </summary>
    [PublicAPI]
    public interface IOvernightSwapApi
    {
        /// <summary>
        /// Retrieve overnight swap calculation history from storage between selected dates.
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        [Post("api/overnightswap/history")]
        Task<IEnumerable<OvernightSwapHistoryContract>> GetOvernightSwapHistory(
            [Query] DateTime from, [Query] DateTime to);

        /// <summary>
        /// Invoke recalculation of account/instrument/direction order packages that were not calculated successfully last time.
        /// </summary>
        [Post("api/overnightswap/recalc.failed.orders")]
        Task RecalculateFailedOrders();
    }
}