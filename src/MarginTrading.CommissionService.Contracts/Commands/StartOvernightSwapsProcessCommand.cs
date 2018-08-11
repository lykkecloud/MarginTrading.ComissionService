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
        
        /// <summary>
        /// Number of financing days to be used in calculation
        /// </summary>
        [Key(2)]
        public int NumberOfFinancingDays { get; }
        
        /// <summary>
        /// Total number of days in the year
        /// </summary>
        [Key(3)]
        public int FinancingDaysPerYear { get; }

        public StartOvernightSwapsProcessCommand([NotNull] string operationId, DateTime creationTimestamp,
            int numberOfFinancingDays, int financingDaysPerYear)
        {
            OperationId = operationId ?? throw new ArgumentNullException(nameof(operationId));
            CreationTimestamp = creationTimestamp;
            NumberOfFinancingDays = numberOfFinancingDays > 0 ? numberOfFinancingDays : throw new ArgumentOutOfRangeException(nameof(numberOfFinancingDays));
            FinancingDaysPerYear = financingDaysPerYear > 0 ? financingDaysPerYear : throw new ArgumentOutOfRangeException(nameof(financingDaysPerYear));
        }
    }
}