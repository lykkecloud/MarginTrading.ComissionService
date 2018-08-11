using System;

namespace MarginTrading.CommissionService.Core.Domain
{
    public class DailyPnlOperationData : CommissionOperationData
    {
        public DateTime TradingDay { get; set; }
    }
}