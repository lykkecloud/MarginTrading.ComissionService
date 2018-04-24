namespace MarginTrading.OvernightSwapService.Infrastructure
{
    public interface ICachedCalculation<out TResult>
    {
        TResult Get();
    }
}