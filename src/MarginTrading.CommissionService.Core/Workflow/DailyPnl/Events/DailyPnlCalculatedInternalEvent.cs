// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using JetBrains.Annotations;
using MessagePack;

namespace MarginTrading.CommissionService.Core.Workflow.DailyPnl.Events
{
    /// <summary>
    /// Event contains information about calculated pnl for a position 
    /// </summary>
    [MessagePackObject]
    public class DailyPnlCalculatedInternalEvent
    {
        /// <summary>
        /// OperationId is produced based on original OperationId from StartDailyPnlProcessCommand
        /// </summary>
        [NotNull]
        [Key(0)]
        public string OperationId { get; }
        
        [Key(1)]
        public DateTime CreationTimestamp { get; }
        
        [NotNull]
        [Key(2)]
        public string AccountId { get; }
        
        [NotNull]
        [Key(3)]
        public string PositionId { get; }
        
        [NotNull]
        [Key(4)]
        public string AssetPairId { get; }
        
        [Key(5)]
        public decimal Pnl { get; }
        
        [Key(6)]
        public DateTime TradingDay { get; }
        
        [Key(7)]
        public decimal Volume { get; }
        
        [Key(8)]
        public decimal FxRate { get; }
        
        [Key(9)]
        public decimal RawTotalPnL { get; }

        public DailyPnlCalculatedInternalEvent([NotNull] string operationId, DateTime creationTimestamp,
            [NotNull] string accountId, [NotNull] string positionId, [NotNull] string assetPairId, decimal pnl, DateTime tradingDay, decimal volume, decimal fxRate, decimal rawTotalPnL)
        {
            OperationId = operationId ?? throw new ArgumentNullException(nameof(operationId));
            CreationTimestamp = creationTimestamp;
            AccountId = accountId ?? throw new ArgumentNullException(nameof(accountId));
            PositionId = positionId ?? throw new ArgumentNullException(nameof(positionId));
            AssetPairId = assetPairId ?? throw new ArgumentNullException(nameof(assetPairId));
            Pnl = pnl;
            TradingDay = tradingDay;
            Volume = volume;
            FxRate = fxRate;
            RawTotalPnL = rawTotalPnL;
        }
    }
}