﻿// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System.Threading.Tasks;

namespace MarginTrading.CommissionService.Core.Services
{
    public interface IBrokerSettingsService
    {
        Task<string> GetSettlementCurrencyAsync();
        Task<bool> IsOrderDetailsReportEnabledAsync();
    }
}