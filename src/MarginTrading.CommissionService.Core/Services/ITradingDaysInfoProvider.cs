// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using MarginTrading.CommissionService.Core.Domain;

namespace MarginTrading.CommissionService.Core.Services
{
    public interface ITradingDaysInfoProvider
    {
        int GetNumberOfNightsUntilNextTradingDay(string marketId, DateTime currentDateTime);

        void Initialize(Dictionary<string, TradingDayInfo> infos);
    }
}