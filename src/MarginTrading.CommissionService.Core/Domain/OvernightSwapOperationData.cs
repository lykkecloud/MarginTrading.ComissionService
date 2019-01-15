namespace MarginTrading.CommissionService.Core.Domain
{
    public class OvernightSwapOperationData : OperationDataBase<CommissionOperationState>
    {
        /// <summary>
        /// Null is for sub-operations
        /// </summary>
        public int? NumberOfFinancingDays { get; set; }
        
        /// <summary>
        /// Null is for sub-operations
        /// </summary>
        public int? FinancingDaysPerYear { get; set; }
    }
}