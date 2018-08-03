using System.Collections.Generic;
using System.Threading.Tasks;
using MarginTrading.CommissionService.Core.Domain.Rates;

namespace MarginTrading.CommissionService.Core.Services
{
    public interface IRateSettingsService
    {
        Task<IReadOnlyList<OrderExecutionRate>> GetOrderExecutionRates();
        Task ReplaceOrderExecutionRates(List<OrderExecutionRate> rates);

        Task<IReadOnlyList<OvernightSwapRate>> GetOvernightSwapRates();
        Task ReplaceOvernightSwapRates(List<OvernightSwapRate> rates);

        Task<OnBehalfRate> GetOnBehalfRate();
        Task ReplaceOnBehalfRate(OnBehalfRate rate);
    }
}