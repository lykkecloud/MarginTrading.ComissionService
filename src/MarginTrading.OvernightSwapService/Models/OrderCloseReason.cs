namespace MarginTrading.OvernightSwapService.Models
{
    public enum OrderCloseReason
    {
        None = 1,
        Close = 2,
        StopLoss = 3,
        TakeProfit = 4,
        StopOut = 5,
        Canceled = 6,
        CanceledBySystem = 7,
        ClosedByBroker = 8,
    }
}