using System.Collections.Generic;
using System.Threading.Tasks;
using MarginTrading.CommissionService.Core.Domain.Abstractions;

namespace MarginTrading.CommissionService.Core.Services
{
    public interface ICommissionCalcService
    {
        Task<(decimal Swap, string Details)> GetOvernightSwap(Dictionary<string, decimal> interestRates,
            IOpenPosition openPosition,
            IAssetPair assetPair, int numberOfFinancingDays, int financingDaysPerYear);
        Task<decimal> CalculateOrderExecutionCommission(string accountId, string instrument, string legalEntity,
            decimal volume, decimal commandOrderExecutionPrice);
        Task<(int ActionsNum, decimal Commission)> CalculateOnBehalfCommissionAsync(string orderId,
            string accountAssetId);
    }
}