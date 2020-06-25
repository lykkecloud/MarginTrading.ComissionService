// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.MarginTrading.CommissionService.Contracts.Models;
using Refit;

namespace Lykke.MarginTrading.CommissionService.Contracts
{
    /// <summary>
    /// Commission rate settings management.
    /// RateSettingsChangedEvent is generated on change.
    /// </summary>
    [PublicAPI]
    public interface IRateSettingsApi
    {
        /// <summary>
        /// Get order execution rates
        /// </summary>
        [Get("/api/rates/get-order-exec")]
        Task<IReadOnlyList<OrderExecutionRateContract>> GetOrderExecutionRates();
        
        /// <summary>
        /// Get order execution rates for asset pair id
        /// </summary>
        [Get("/api/rates/get-order-exec/{assetPairId}")]
        Task<OrderExecutionRateContract> GetOrderExecutionRate(string assetPairId);
        
        /// <summary>
        /// Get order execution rates for the list of asset pair ids
        /// </summary>
        /// <param name="assetPairIds">The list of asset pair ids</param>
        /// <returns></returns>
        [Get("/api/rates/get-order-exec/list")]
        Task<IReadOnlyList<OrderExecutionRateContract>> GetOrderExecutionRates(string[] assetPairIds);

        /// <summary>
        /// Insert or update existing order execution rates
        /// </summary>
        [Post("/api/rates/replace-order-exec")]
        Task ReplaceOrderExecutionRates([Body, NotNull] OrderExecutionRateContract[] rates);
        
        /// <summary>
        /// Get overnight swap rates
        /// </summary>
        [Get("/api/rates/get-overnight-swap")]
        Task<IReadOnlyList<OvernightSwapRateContract>> GetOvernightSwapRates();

        /// <summary>
        /// Insert or update existing overnight swap rates
        /// </summary>
        [Post("/api/rates/replace-overnight-swap")]
        Task ReplaceOvernightSwapRates([Body, NotNull] OvernightSwapRateContract[] rates);
        
        /// <summary>
        /// Get on behalf rate
        /// </summary>
        [Get("/api/rates/get-on-behalf")]
        [ItemCanBeNull]
        Task<OnBehalfRateContract> GetOnBehalfRate();

        /// <summary>
        /// Insert or update existing on behalf rate
        /// </summary>
        [Post("/api/rates/replace-on-behalf")]
        Task ReplaceOnBehalfRate([Body, NotNull] OnBehalfRateContract rate);
    }
}