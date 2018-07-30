using System;
using JetBrains.Annotations;
using MarginTrading.CommissionService.Core.Domain.Abstractions;

namespace MarginTrading.CommissionService.Core.Domain
{
    public class DailyPnlCalculation : IDailyPnlCalculation
    {
        public string Id => GetId(OperationId, PositionId);
        
        public string OperationId { get; }
        public string AccountId { get; }
        public string Instrument { get; }
        public DateTime TradingDay { get; }
        public decimal Volume { get; }
        public decimal FxRate { get; }
        public string PositionId { get; }
        public decimal Pnl { get; }

        public DailyPnlCalculation([NotNull] string operationId, [NotNull] string accountId,
            [NotNull] string instrument, DateTime tradingDay, decimal volume, decimal fxRate,
            [NotNull] string positionId, decimal pnl)
        {
            OperationId = operationId ?? throw new ArgumentNullException(nameof(operationId));;
            AccountId = accountId ?? throw new ArgumentNullException(nameof(operationId));;
            Instrument = instrument ?? throw new ArgumentNullException(nameof(operationId));;
            TradingDay = tradingDay;
            Volume = volume;
            FxRate = fxRate;
            PositionId = positionId ?? throw new ArgumentNullException(nameof(operationId));
            Pnl = pnl;
        }

        public static string GetId(string operationId, string positionId) => $"{operationId}_{positionId}";
    }
}