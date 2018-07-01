namespace MarginTrading.CommissionService.Core.Domain
{
    public enum OrderType
    {
        Market = 1,
        Limit = 2,
        Stop = 3,
        TakeProfit = 4,
        StopLoss = 5,
        TrailingStop = 6,
    }
}