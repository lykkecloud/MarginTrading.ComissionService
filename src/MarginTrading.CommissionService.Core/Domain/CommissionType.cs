namespace MarginTrading.CommissionService.Core.Domain
{
    public enum CommissionType
    {
        OrderExecution = 1,
        OnBehalf = 2,
        OvernightSwap = 3,
        UnrealizedPnl = 4,
    }
}