// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System.Threading.Tasks;
using MarginTrading.CommissionService.Core.Domain;

namespace MarginTrading.CommissionService.Core.Services
{
    public interface ICommissionCalcService
    {
        Task<(decimal Swap, string Details)> GetOvernightSwap(string accountId, string instrument,
            decimal volume, decimal closePrice, PositionDirection direction,
            int numberOfFinancingDays, int financingDaysPerYear);
        Task<decimal> CalculateOrderExecutionCommission(string accountId, string instrument,
            decimal volume, decimal commandOrderExecutionPrice, decimal orderExecutionFxRate);
        Task<(int ActionsNum, decimal Commission)> CalculateOnBehalfCommissionAsync(string orderId,
            string accountAssetId, string assetPairId);
    }
}