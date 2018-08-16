namespace MarginTrading.CommissionService.Core.Domain
{
    public class OnBehalfOperationData: CommissionOperationData
    {
        public string AccountId { get; set; }
        
        public string OrderId { get; set; }
    }
}