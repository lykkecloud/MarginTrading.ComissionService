namespace MarginTrading.OvernightSwapService.Models
{
    public enum OrderStatus
    {
        WaitingForExecution = 1,
        Active = 2,
        Closed = 3,
        Rejected = 4,
        Closing = 5,
    }
}