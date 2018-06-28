using System;
using JetBrains.Annotations;
using MessagePack;

namespace Lykke.MarginTrading.CommissionService.Contracts.Commands
{
    /// <summary>
    /// Command to perform overnight swaps calculation and account charging
    /// </summary>
    [MessagePackObject]
    public class StartOvernightSwapsProcessCommand
    {
        /// <summary>
        /// Unique operation id, GUID is recommended
        /// </summary>
        [Key(0)]
        [NotNull] public string OperationId { get; }
        
        /// <summary>
        /// Command creation timestamp
        /// </summary>
        [Key(1)]
        public DateTime CreationTimestamp { get; }

        public StartOvernightSwapsProcessCommand([NotNull] string operationId, DateTime creationTimestamp)
        {
            OperationId = operationId ?? throw new ArgumentNullException(nameof(operationId));
            CreationTimestamp = creationTimestamp;
        }
    }
}