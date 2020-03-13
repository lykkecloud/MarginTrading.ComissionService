// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using JetBrains.Annotations;
using MessagePack;

namespace MarginTrading.CommissionService.Core.Workflow.ChargeCommission.Commands
{
    /// <summary>
    /// Command to calculate order execution commissions
    /// </summary>
    [MessagePackObject]
    public class HandleOrderExecInternalCommand
    {
        public HandleOrderExecInternalCommand([NotNull] string operationId,
            [NotNull] string accountId, [NotNull] string orderId, long orderCode, 
            [NotNull] string instrument, [NotNull] string legalEntity, decimal volume, 
            DateTime tradingDay, decimal orderExecutionPrice, decimal orderExecutionFxRate)
        {
            OperationId = operationId ?? throw new ArgumentNullException(nameof(operationId));
            AccountId = accountId ?? throw new ArgumentNullException(nameof(accountId));
            OrderId = orderId ?? throw new ArgumentNullException(nameof(orderId));
            OrderCode = orderCode;
            Instrument = instrument ?? throw new ArgumentNullException(nameof(instrument));
            LegalEntity = legalEntity ?? throw new ArgumentNullException(nameof(legalEntity));
            Volume = volume;
            OrderExecutionPrice = orderExecutionPrice;
            OrderExecutionFxRate = orderExecutionFxRate;
            TradingDay = tradingDay == default ? DateTime.UtcNow : tradingDay;
        }

        /// <summary>
        /// Unique operation ID
        /// </summary>
        [NotNull]
        [Key(0)]
        public string OperationId { get; }

        /// <summary>
        /// Trading account ID
        /// </summary>
        [NotNull]
        [Key(1)]
        public string AccountId { get; }

        /// <summary>
        /// Order ID
        /// </summary>
        [Key(2)]
        public string OrderId { get; }
        
        /// <summary>
        /// Digit order code
        /// </summary>
        [Key(3)]
        public long OrderCode { get; }
        
        /// <summary>
        /// Asset Pair ID (eg. EURUSD) 
        /// </summary>
        [Key(4)]
        public string Instrument { get; }
        
        /// <summary>
        /// LegalEntity
        /// </summary>
        [Key(5)]
        public string LegalEntity { get; }
        
        /// <summary>
        /// Order size 
        /// </summary>
        [Key(6)]
        public decimal Volume { get; }
        
        /// <summary>
        /// Trading day. If not passed current DateTime.UtcNow will be used.
        /// </summary>
        [Key(7)]
        public DateTime TradingDay { get; }
        
        /// <summary>
        /// Order execution price
        /// </summary>
        [Key(8)]
        public decimal OrderExecutionPrice { get; }
        
        /// <summary>
        /// Order execution fx rate
        /// </summary>
        [Key(9)]
        public decimal OrderExecutionFxRate { get; }
    }
}