using System;

namespace MarginTrading.CommissionService.Core.Domain
{
    public class DailyPnlOperationData : OperationDataBase<CommissionOperationState>
    {
        public DateTime TradingDay { get; set; }
    }
}