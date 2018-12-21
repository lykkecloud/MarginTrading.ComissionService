namespace MarginTrading.CommissionService.Core.Domain
{
    public class OnBehalfOperationData: OperationDataBase<CommissionOperationState>
    {
        public string AccountId { get; set; }
        
        public string OrderId { get; set; }
    }
}