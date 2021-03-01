// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System.Threading.Tasks;
using JetBrains.Annotations;
using MarginTrading.CommissionService.Core.Domain.Rates;


namespace MarginTrading.CommissionService.Core.Services
{
    public interface IRateSettingsCache
    {
        Task<OvernightSwapRate> GetOvernightSwapRate([NotNull] string assetPair, string tradingConditionId);
        Task ClearOvernightSwapRatesCache();
        Task<OnBehalfRate> GetOnBehalfRate(string accountId, string assetType);
    }
}