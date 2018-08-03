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
    public interface IRates
    {
        /// <summary>
        /// Get order execution rates
        /// </summary>
        [Get("api/rates/get-order-exec")]
        Task<IReadOnlyList<OrderExecutionRateContract>> GetOrderExecutionRates();

        /// <summary>
        /// Insert or update existing order execution rates
        /// </summary>
        [Post("api/rates/replace-order-exec")]
        Task ReplaceOrderExecutionRates([Query, NotNull] List<OrderExecutionRateContract> rates);
        
        
        
        /// <summary>
        /// Get overnight swap rates
        /// </summary>
        [Get("api/rates/get-overnight-swap")]
        Task<IReadOnlyList<OvernightSwapRateContract>> GetOvernightSwapRates();

        /// <summary>
        /// Insert or update existing overnight swap rates
        /// </summary>
        [Post("api/rates/replace-overnight-swap")]
        Task ReplaceOvernightSwapRates([Query, NotNull] List<OvernightSwapRateContract> rates);
        
        
        
        /// <summary>
        /// Get on behalf rate
        /// </summary>
        [Get("api/rates/get-on-behalf")]
        [ItemCanBeNull]
        Task<OnBehalfRateContract> GetOnBehalfRate();

        /// <summary>
        /// Insert or update existing on behalf rate
        /// </summary>
        [Post("api/rates/replace-on-behalf")]
        Task ReplaceOnBehalfRate([Query, NotNull] OnBehalfRateContract rate);
    }
}