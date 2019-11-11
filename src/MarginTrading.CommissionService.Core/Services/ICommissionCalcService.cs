// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Threading.Tasks;
using MarginTrading.CommissionService.Core.Domain;
using MarginTrading.CommissionService.Core.Domain.Abstractions;

namespace MarginTrading.CommissionService.Core.Services
{
    public interface ICommissionCalcService
    {
        Task<(decimal Swap, string Details)> GetOvernightSwap(string accountId, string instrument,
            decimal volume, decimal closePrice, PositionDirection direction,
            int numberOfFinancingDays, int financingDaysPerYear);
        Task<decimal> CalculateOrderExecutionCommission(string accountId, string instrument,
            decimal volume, decimal commandOrderExecutionPrice);
        Task<(int ActionsNum, decimal Commission)> CalculateOnBehalfCommissionAsync(string orderId,
            string accountAssetId, string assetPairId);
    }
}