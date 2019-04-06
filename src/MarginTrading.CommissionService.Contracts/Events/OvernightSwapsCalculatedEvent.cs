using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using MessagePack;

namespace Lykke.MarginTrading.CommissionService.Contracts.Events
{
    /// <summary>
    /// Event indicates that overnight swap calculation has finished 
    /// </summary>
    [MessagePackObject]
    public class OvernightSwapsCalculatedEvent
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
        /// Total number of overnight swaps calculated. Equals to the number of positions involved.
        /// </summary>
        [Key(2)]
        public int Total { get; }
        
        /// <summary>
        /// Number of failed calculations
        /// </summary>
        [Key(3)]
        public int Failed { get; }

        public OvernightSwapsCalculatedEvent([NotNull] string operationId, DateTime creationTimestamp, int total,
            int failed)
        {
            OperationId = operationId ?? throw new ArgumentNullException(nameof(operationId));
            CreationTimestamp = creationTimestamp;
            Total = total;
            Failed = failed;
        }
    }
}