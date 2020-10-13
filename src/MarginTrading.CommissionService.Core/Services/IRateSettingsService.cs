// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MarginTrading.CommissionService.Core.Domain.Rates;


namespace MarginTrading.CommissionService.Core.Services
{
    public interface IRateSettingsService
    {
        Task<IReadOnlyList<OrderExecutionRate>> GetOrderExecutionRates(IList<string> assetPairIds = null);
        Task<OvernightSwapRate> GetOvernightSwapRate([NotNull] string assetPair);
        Task<IReadOnlyList<OvernightSwapRate>> GetOvernightSwapRatesForApi();
        OnBehalfRate GetOnBehalfRate(string assetType);
    }
}