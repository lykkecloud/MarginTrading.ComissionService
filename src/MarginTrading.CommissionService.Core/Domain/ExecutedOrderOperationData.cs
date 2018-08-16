namespace MarginTrading.CommissionService.Core.Domain
{
    public class ExecutedOrderOperationData : CommissionOperationData
    {
        public string AccountId { get; set; }
        
        public string OrderId { get; set; }
        
        public long OrderCode { get; set; }
        
        public string Instrument { get; set; }
        
        public string LegalEntity { get; set; }
        
        public decimal Volume { get; set; }
    }
}