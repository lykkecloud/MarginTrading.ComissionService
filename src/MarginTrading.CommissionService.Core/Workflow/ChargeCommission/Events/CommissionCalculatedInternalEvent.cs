using System;
using JetBrains.Annotations;
using MarginTrading.CommissionService.Core.Domain;
using MessagePack;

namespace MarginTrading.CommissionService.Core.Workflow.ChargeCommission.Events
{
    [MessagePackObject]
    public class CommissionCalculatedInternalEvent
    {
        public CommissionCalculatedInternalEvent([NotNull] string operationId,
            [NotNull] string accountId, [NotNull] string orderId, decimal amount, CommissionType commissionType,
            [NotNull] string reason)
        {
            OperationId = operationId ?? throw new ArgumentNullException(nameof(operationId));
            AccountId = accountId ?? throw new ArgumentNullException(nameof(accountId));
            OrderId = orderId ?? throw new ArgumentNullException(nameof(orderId));
            Amount = amount;
            CommissionType = commissionType;
            Reason = reason ?? throw new ArgumentNullException(nameof(reason));
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
        [NotNull]
        [Key(2)]
        public string OrderId { get; }
        
        /// <summary>
        /// Commission amount
        /// </summary>
        [Key(3)]
        public decimal Amount { get; }
        
        /// <summary>
        /// Type of calculated commission
        /// </summary>
        [Key(4)]
        public CommissionType CommissionType { get; }

        /// <summary>
        /// Commission reason
        /// </summary>
        [NotNull]
        [Key(5)]
        public string Reason { get; }
    }
}