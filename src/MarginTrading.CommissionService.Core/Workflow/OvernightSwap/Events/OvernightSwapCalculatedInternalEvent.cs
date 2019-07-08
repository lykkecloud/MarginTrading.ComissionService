// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using JetBrains.Annotations;
using MarginTrading.CommissionService.Core.Extensions;
using MessagePack;

namespace MarginTrading.CommissionService.Core.Workflow.OvernightSwap.Events
{
    /// <summary>
    /// Event contains information about commission for a single position 
    /// </summary>
    [MessagePackObject]
    public class OvernightSwapCalculatedInternalEvent
    {
        /// <summary>
        /// OperationId is produced based on original OperationId from StartOvernightSwapsProcessCommand
        /// It is produced as {OperationId}_{swap stream number}
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
        public decimal SwapAmount { get; }
        
        [Key(6)]
        public string Details { get; }
        
        [Key(7)]
        public DateTime TradingDay { get; }

        public OvernightSwapCalculatedInternalEvent([NotNull] string operationId, DateTime creationTimestamp,
            [NotNull] string accountId, [NotNull] string positionId, [NotNull] string assetPairId, decimal swapAmount,
            string details, DateTime tradingDay)
        {
            OperationId = operationId ?? throw new ArgumentNullException(nameof(operationId));
            CreationTimestamp = creationTimestamp;
            AccountId = accountId ?? throw new ArgumentNullException(nameof(accountId));
            PositionId = positionId ?? throw new ArgumentNullException(nameof(positionId));
            AssetPairId = assetPairId ?? throw new ArgumentNullException(nameof(assetPairId));
            SwapAmount = swapAmount;
            Details = details;
            TradingDay = tradingDay;
        }
    }
}