using MarginTrading.OvernightSwapService.Models.Abstractions;

namespace MarginTrading.OvernightSwapService.Services
{
    public interface ICommissionService
    {
        decimal GetOvernightSwap(IOrder order, decimal swapRate);
    }
}