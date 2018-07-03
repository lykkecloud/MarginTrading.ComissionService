namespace MarginTrading.CommissionService.Core.Domain.Rates
{
    public class OrderExecutionRate
    {
        public decimal CommissionCap { get; }
        
        public decimal CommissionFloor { get; }
        
        public decimal CommissionRate { get; }

        public OrderExecutionRate(decimal commissionCap, decimal commissionFloor, decimal commissionRate)
        {
            CommissionCap = commissionCap;
            CommissionFloor = commissionFloor;
            CommissionRate = commissionRate;
        }
    }
}