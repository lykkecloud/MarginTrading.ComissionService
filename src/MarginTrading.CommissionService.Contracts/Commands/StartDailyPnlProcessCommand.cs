// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using JetBrains.Annotations;
using MessagePack;

namespace Lykke.MarginTrading.CommissionService.Contracts.Commands
{
    /// <summary>
    /// Command to perform daily pnl calculation and account charging
    /// </summary>
    [MessagePackObject]
    public class StartDailyPnlProcessCommand
    {
        /// <summary>
        /// Unique operation id, GUID is recommended
        /// </summary>
        [Key(0)]
        [NotNull] public string OperationId { get; }
        
        /// <summary>
        /// Command creation timestamp
        /// </summary>
        [Key(1)]
        public DateTime CreationTimestamp { get; }
        
        /// <summary>
        /// Command creation timestamp
        /// </summary>
        [Key(2)]
        public DateTime TradingDay { get; }

        public StartDailyPnlProcessCommand([NotNull] string operationId, DateTime creationTimestamp, DateTime tradingDay)
        {
            OperationId = operationId ?? throw new ArgumentNullException(nameof(operationId));
            CreationTimestamp = creationTimestamp;
            TradingDay = tradingDay;
        }
    }
}