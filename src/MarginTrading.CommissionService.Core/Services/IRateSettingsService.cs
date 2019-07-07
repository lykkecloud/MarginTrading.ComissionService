using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MarginTrading.CommissionService.Core.Domain.Rates;

namespace MarginTrading.CommissionService.Core.Services
{
    public interface IRateSettingsService
    {
        Task<OrderExecutionRate> GetOrderExecutionRate([NotNull] string tradingConditionId, [NotNull] string assetPairId);
        Task<IReadOnlyList<OrderExecutionRate>> GetOrderExecutionRates(string tradingConditionId = null);
        Task ReplaceOrderExecutionRates(List<OrderExecutionRate> rates);
        Task DeleteOrderExecutionRates(List<OrderExecutionRate> rates);

        Task<OvernightSwapRate> GetOvernightSwapRate([NotNull] string assetPair);
        Task<IReadOnlyList<OvernightSwapRate>> GetOvernightSwapRatesForApi();
        Task ReplaceOvernightSwapRates(List<OvernightSwapRate> rates);
        Task DeleteOvernightSwapRates(List<OvernightSwapRate> rates);

        Task<OnBehalfRate> GetOnBehalfRate([NotNull] string tradingConditionId);
        Task<IReadOnlyList<OnBehalfRate>> GetOnBehalfRates();
        Task ReplaceOnBehalfRates(List<OnBehalfRate> rates);
        Task DeleteOnBehalfRates(List<OnBehalfRate> rates);
    }
}