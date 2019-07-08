// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using JetBrains.Annotations;
using MessagePack;

namespace Lykke.MarginTrading.CommissionService.Contracts.Events
{
    /// <summary>
    /// Event indicates that daily PnLs calculation has finished 
    /// </summary>
    [MessagePackObject]
    public class DailyPnlsCalculatedEvent
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
        /// Total number of daily PnLs calculated. Equals to the number of positions involved.
        /// </summary>
        [Key(2)]
        public int Total { get; }
        
        /// <summary>
        /// Number of failed calculations
        /// </summary>
        [Key(3)]
        public int Failed { get; }

        public DailyPnlsCalculatedEvent([NotNull] string operationId, DateTime creationTimestamp, int total,
            int failed)
        {
            OperationId = operationId ?? throw new ArgumentNullException(nameof(operationId));
            CreationTimestamp = creationTimestamp;
            Total = total;
            Failed = failed;
        }
    }
}