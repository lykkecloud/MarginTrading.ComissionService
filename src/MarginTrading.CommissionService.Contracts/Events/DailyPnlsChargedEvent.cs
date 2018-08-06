using System;
using JetBrains.Annotations;
using MessagePack;

namespace Lykke.MarginTrading.CommissionService.Contracts.Events
{
    /// <summary>
    /// Event indicates that daily PnLs were charged from accounts 
    /// </summary>
    [MessagePackObject]
    public class DailyPnlsChargedEvent
    {
        /// <summary>
        /// Unique operation id
        /// </summary>
        [NotNull]
        [Key(0)]
        public string OperationId { get; }
        
        /// <summary>
        /// Event creation timestamp
        /// </summary>
        [Key(1)]
        public DateTime CreationTimestamp { get; }
        
        /// <summary>
        /// Total number of daily PnLs that was trying to be charged
        /// </summary>
        [Key(2)]
        public int Total { get; }
        
        /// <summary>
        /// Number of daily PnLs that was not charged from account during corresponding time period
        /// </summary>
        [Key(3)]
        public int Failed { get; }

        public DailyPnlsChargedEvent([NotNull] string operationId, DateTime creationTimestamp, int total,
            int failed)
        {
            OperationId = operationId ?? throw new ArgumentNullException(nameof(operationId));
            CreationTimestamp = creationTimestamp;
            Total = total;
            Failed = failed;
        }
    }
}