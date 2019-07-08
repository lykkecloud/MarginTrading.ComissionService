// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;

namespace MarginTrading.CommissionService.Core.Domain
{
    public class DailyPnlOperationData : OperationDataBase<CommissionOperationState>
    {
        public DateTime TradingDay { get; set; }
    }
}