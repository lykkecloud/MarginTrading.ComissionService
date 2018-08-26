using System;
using JetBrains.Annotations;
using MessagePack;

namespace MarginTrading.CommissionService.Core.Workflow.OnBehalf.Events
{
    [MessagePackObject]
    public class OnBehalfCalculatedInternalEvent
    {
        /// <summary>
        /// Unique operation ID
        /// </summary>
        [NotNull]
        [Key(0)]
        public string OperationId { get; }
        
        /// <summary>
        /// Command creation time stamp
        /// </summary>
        [Key(1)]
        public DateTime CreatedTimestamp { get; }

        /// <summary>
        /// Trading account ID
        /// </summary>
        [NotNull]
        [Key(2)]
        public string AccountId { get; }

        /// <summary>
        /// Order ID
        /// </summary>
        [NotNull]
        [Key(3)]
        public string OrderId { get; }
        
        /// <summary>
        /// Asset pair ID
        /// </summary>
        [NotNull]
        [Key(4)]
        public string AssetPairId { get; }
        
        /// <summary>
        /// Number of on behalf actions applied
        /// </summary>
        [Key(5)]
        public int NumberOfActions { get; }
        
        /// <summary>
        /// Commission amount to be charged to account
        /// </summary>
        [Key(6)]
        public decimal Commission { get; }
        
        /// <summary>
        /// Trading day.
        /// </summary>
        [Key(7)]
        public DateTime TradingDay { get; }

        public OnBehalfCalculatedInternalEvent([NotNull] string operationId, DateTime createdTimestamp,
            [NotNull] string accountId, [NotNull] string orderId, [NotNull] string assetPairId, int numberOfActions, 
            decimal commission, DateTime tradingDay)
        {
            OperationId = operationId ?? throw new ArgumentNullException(nameof(operationId));
            CreatedTimestamp = createdTimestamp;
            AccountId = accountId ?? throw new ArgumentNullException(nameof(accountId));
            OrderId = orderId ?? throw new ArgumentNullException(nameof(orderId));
            AssetPairId = assetPairId ?? throw new ArgumentNullException(nameof(assetPairId));
            NumberOfActions = numberOfActions;
            Commission = commission;
            TradingDay = tradingDay;
        }
    }
}