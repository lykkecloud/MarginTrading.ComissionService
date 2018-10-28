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
        
        /// <summary>
        /// Trading day for overnight swaps. If not passed current DateTime.UtcNow will be used.
        /// </summary>
        [Key(4)]
        public DateTime TradingDay { get; }

        public StartOvernightSwapsProcessCommand([NotNull] string operationId, DateTime creationTimestamp, 
            int numberOfFinancingDays = 0, int financingDaysPerYear = 0, DateTime tradingDay = default)
        {
            OperationId = operationId ?? throw new ArgumentNullException(nameof(operationId));
            CreationTimestamp = creationTimestamp;
            //TODO this hardcode may be removed after integration is finished
            NumberOfFinancingDays = numberOfFinancingDays > 0 ? numberOfFinancingDays : 1;
            FinancingDaysPerYear = financingDaysPerYear > 0 ? financingDaysPerYear : 365;
            
            TradingDay = tradingDay == default ? DateTime.UtcNow.Date : tradingDay.Date;
        }
    }
}