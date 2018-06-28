namespace MarginTrading.CommissionService.Core.Domain.Rates
{
    public class OrderExecution
    {
        public decimal CommissionCap { get; }
        
        public decimal CommissionFloor { get; }
        
        public decimal CommissionRate { get; }

        public OrderExecution(decimal commissionCap, decimal commissionFloor, decimal commissionRate)
        {
            CommissionCap = commissionCap;
            CommissionFloor = commissionFloor;
            CommissionRate = commissionRate;
        }
    }
}