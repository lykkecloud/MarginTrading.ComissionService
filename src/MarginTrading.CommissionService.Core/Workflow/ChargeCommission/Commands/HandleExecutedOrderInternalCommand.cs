using System;
using JetBrains.Annotations;
using MessagePack;

namespace MarginTrading.CommissionService.Core.Workflow.ChargeCommission.Commands
{
    /// <summary>
    /// Command to calculate order execution commissions
    /// </summary>
    [MessagePackObject]
    public class HandleExecutedOrderInternalCommand
    {
        public HandleExecutedOrderInternalCommand([NotNull] string operationId,
            [NotNull] string accountId, [NotNull] string orderId, long orderCode, 
            [NotNull] string instrument, [NotNull] string legalEntity, decimal volume)
        {
            OperationId = operationId ?? throw new ArgumentNullException(nameof(operationId));
            AccountId = accountId ?? throw new ArgumentNullException(nameof(accountId));
            OrderId = orderId ?? throw new ArgumentNullException(nameof(orderId));
            OrderCode = orderCode;
            Instrument = instrument ?? throw new ArgumentNullException(nameof(instrument));
            LegalEntity = legalEntity ?? throw new ArgumentNullException(nameof(legalEntity));
            Volume = volume;
        }

        /// <summary>
        /// Unique opetation ID
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
    }
}