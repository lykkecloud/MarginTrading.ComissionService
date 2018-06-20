using MarginTrading.CommissionService.Core.Domain.Abstractions;

namespace MarginTrading.CommissionService.Core.Services
{
    public interface ICommissionService
    {
        decimal GetOvernightSwap(IOrder order, decimal swapRate);
    }
}