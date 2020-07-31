// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Threading.Tasks;
using MarginTrading.AssetService.Contracts.Rates;

namespace MarginTrading.CommissionService.Core.Services
{
    public interface IRateSettingsCache
    {
        Task<List<OrderExecutionRateContract>> RefreshOrderExecutionRates();

        Task<List<OvernightSwapRateContract>> RefreshOvernightSwapRates();

        Task<OnBehalfRateContract> RefreshOnBehalfRate();
    }
}