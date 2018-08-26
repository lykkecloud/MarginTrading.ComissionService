using System;
using JetBrains.Annotations;
using MessagePack;

namespace MarginTrading.CommissionService.Core.Workflow.OnBehalf.Commands
{
    /// <summary>
    /// Command to calculate on behalf commissions
    /// </summary>
    [MessagePackObject]
    public class HandleOnBehalfInternalCommand
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
        /// Account asset ID
        /// </summary>
        [NotNull]
        [Key(3)]
        public string AccountAssetId { get; }

        /// <summary>
        /// Order ID
        /// </summary>
        [NotNull]
        [Key(4)]
        public string OrderId { get; }
        
        /// <summary>
        /// Asset pair ID
        /// </summary>
        [NotNull]
        [Key(5)]
        public string AssetPairId { get; }
        
        /// <summary>
        /// Trading day. If not passed current DateTime.UtcNow will be used.
        /// </summary>
        [Key(6)]
        public DateTime TradingDay { get; }

        public HandleOnBehalfInternalCommand([NotNull] string operationId, DateTime createdTimestamp,
            [NotNull] string accountId, [NotNull] string accountAssetId, [NotNull] string orderId, 
            [NotNull] string assetPairId, DateTime tradingDay = default)
        {
            OperationId = operationId ?? throw new ArgumentNullException(nameof(operationId));
            CreatedTimestamp = createdTimestamp;
            AccountId = accountId ?? throw new ArgumentNullException(nameof(accountId));
            AccountAssetId = accountAssetId ?? throw new ArgumentNullException(nameof(accountAssetId));
            OrderId = orderId ?? throw new ArgumentNullException(nameof(orderId));
            AssetPairId = assetPairId ?? throw new ArgumentNullException(nameof(assetPairId));
            TradingDay = tradingDay == default ? DateTime.UtcNow : tradingDay;
        }
    }
}