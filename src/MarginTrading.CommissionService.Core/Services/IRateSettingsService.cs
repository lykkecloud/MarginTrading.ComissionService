using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MarginTrading.CommissionService.Core.Domain.Rates;

namespace MarginTrading.CommissionService.Core.Services
{
    public interface IRateSettingsService
    {
        Task<OrderExecutionRate> GetOrderExecutionRate([NotNull] string assetPairId);
        Task<IReadOnlyList<OrderExecutionRate>> GetOrderExecutionRatesForApi();
        Task ReplaceOrderExecutionRates(List<OrderExecutionRate> rates);

        Task<OvernightSwapRate> GetOvernightSwapRate([NotNull] string assetPair);
        Task<IReadOnlyList<OvernightSwapRate>> GetOvernightSwapRatesForApi();
        Task ReplaceOvernightSwapRates(List<OvernightSwapRate> rates);

        Task<OnBehalfRate> GetOnBehalfRate();
        Task<OnBehalfRate> GetOnBehalfRateApi();
        Task ReplaceOnBehalfRate(OnBehalfRate rate);
    }
}