// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using JetBrains.Annotations;
using MessagePack;

namespace MarginTrading.CommissionService.Contracts.Events
{
    /// <summary>
    /// Event indicates that error happened during daily PnLs calculation initialization
    /// </summary>
    [MessagePackObject]
    public class DailyPnlsStartFailedEvent
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
        /// Fail reason
        /// </summary>
        [NotNull]
        [Key(2)]
        public string FailReason { get; }

        public DailyPnlsStartFailedEvent([NotNull] string operationId, DateTime creationTimestamp,
            [NotNull] string failReason)
        {
            OperationId = operationId ?? throw new ArgumentNullException(nameof(operationId));
            CreationTimestamp = creationTimestamp;
            FailReason = failReason ?? throw new ArgumentNullException(nameof(failReason));
        }
    }
}