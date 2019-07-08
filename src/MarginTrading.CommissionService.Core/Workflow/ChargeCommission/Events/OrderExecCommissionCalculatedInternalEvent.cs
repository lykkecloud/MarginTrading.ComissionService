// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using JetBrains.Annotations;
using MarginTrading.CommissionService.Core.Domain;
using MessagePack;

namespace MarginTrading.CommissionService.Core.Workflow.ChargeCommission.Events
{
    [MessagePackObject]
    public class OrderExecCommissionCalculatedInternalEvent
    {
        public OrderExecCommissionCalculatedInternalEvent([NotNull] string operationId, [NotNull] string accountId, 
            [NotNull] string orderId, [CanBeNull] string assetPairId, decimal amount, CommissionType commissionType,
            [NotNull] string reason, DateTime tradingDay)
        {
            OperationId = operationId ?? throw new ArgumentNullException(nameof(operationId));
            AccountId = accountId ?? throw new ArgumentNullException(nameof(accountId));
            OrderId = orderId ?? throw new ArgumentNullException(nameof(orderId));
            AssetPairId = assetPairId;
            Amount = amount;
            CommissionType = commissionType;
            Reason = reason ?? throw new ArgumentNullException(nameof(reason));
            TradingDay = tradingDay;
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
        [NotNull]
        [Key(2)]
        public string OrderId { get; }
        
        /// <summary>
        /// AssetPair Id for commission calculation. Set if applicable.
        /// </summary>
        [CanBeNull]
        [Key(3)]
        public string AssetPairId { get; }
        
        /// <summary>
        /// Commission amount
        /// </summary>
        [Key(4)]
        public decimal Amount { get; }
        
        /// <summary>
        /// Type of calculated commission
        /// </summary>
        [Key(5)]
        public CommissionType CommissionType { get; }

        /// <summary>
        /// Commission reason
        /// </summary>
        [NotNull]
        [Key(6)]
        public string Reason { get; }
        
        /// <summary>
        /// Trading day.
        /// </summary>
        [Key(7)]
        public DateTime TradingDay { get; }
    }
}