namespace MarginTrading.CommissionService.Core.Services
{
    public interface ICachedCalculation<out TResult>
    {
        TResult Get();
    }
}