// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MarginTrading.AssetService.Contracts.Rates;

namespace MarginTrading.CommissionService.Core.Services
{
    public interface IRateSettingsService
    {
        Task<IReadOnlyList<OrderExecutionRateContract>> GetOrderExecutionRates(IList<string> assetPairIds = null);
        Task ReplaceOrderExecutionRates(List<OrderExecutionRateContract> rates);
        Task<OvernightSwapRateContract> GetOvernightSwapRate([NotNull] string assetPair);
        Task<IReadOnlyList<OvernightSwapRateContract>> GetOvernightSwapRatesForApi();
        Task ReplaceOvernightSwapRates(List<OvernightSwapRateContract> rates);
        Task<OnBehalfRateContract> GetOnBehalfRate();
        Task ReplaceOnBehalfRate(OnBehalfRateContract rate);
    }
}