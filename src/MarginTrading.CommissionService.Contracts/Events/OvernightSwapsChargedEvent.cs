// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using JetBrains.Annotations;
using MessagePack;

namespace MarginTrading.CommissionService.Contracts.Events
{
    /// <summary>
    /// Event indicates that overnight swaps were charged from accounts 
    /// </summary>
    [MessagePackObject]
    public class OvernightSwapsChargedEvent
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
        /// Total number of overnight swaps that was trying to be charged
        /// </summary>
        [Key(2)]
        public int Total { get; }
        
        /// <summary>
        /// Number of swaps that was not charged from account during corresponding time period
        /// </summary>
        [Key(3)]
        public int Failed { get; }

        public OvernightSwapsChargedEvent([NotNull] string operationId, DateTime creationTimestamp, int total,
            int failed)
        {
            OperationId = operationId ?? throw new ArgumentNullException(nameof(operationId));
            CreationTimestamp = creationTimestamp;
            Total = total;
            Failed = failed;
        }
    }
}