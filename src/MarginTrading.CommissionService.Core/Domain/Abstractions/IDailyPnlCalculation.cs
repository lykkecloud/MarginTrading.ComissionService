using System;

namespace MarginTrading.CommissionService.Core.Domain.Abstractions
{
    public interface IDailyPnlCalculation
    {
        string OperationId { get; }
        string AccountId { get; }
        string Instrument { get; }
        DateTime TradingDay { get; }
        decimal Volume { get; }
        decimal FxRate { get; }
        string PositionId { get; }
        decimal Pnl { get; }

        string GetId();
    }
}