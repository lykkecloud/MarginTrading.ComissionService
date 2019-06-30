using System;
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
        /// Get on behalf rate. If accountId not set Trading Profile value is returned.
        /// </summary>
        [Get("/api/rates/get-on-behalf")]
        [ItemCanBeNull]
        Task<OnBehalfRateContract> GetOnBehalfRate([Query, NotNull] string accountId = "");
        
        /// <summary>
        /// Get all on behalf rates
        /// </summary>
        [Get("/api/rates/get-all-on-behalf")]
        [ItemCanBeNull]
        Task<IReadOnlyList<OnBehalfRateContract>> GetOnBehalfRates();

        /// <summary>
        /// Insert or update existing on behalf rate
        /// </summary>
        [Obsolete("Use replace-all-on-behalf")]
        [Post("/api/rates/replace-on-behalf")]
        Task ReplaceOnBehalfRate([Body, NotNull] OnBehalfRateContract rate);

        /// <summary>
        /// Insert or update all on behalf rates
        /// </summary>
        [Post("/api/rates/replace-all-on-behalf")]
        Task ReplaceAllOnBehalfRate([Body, NotNull] List<OnBehalfRateContract> rates);
    }
}