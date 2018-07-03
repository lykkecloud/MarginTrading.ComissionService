using MarginTrading.CommissionService.Core.Domain.Abstractions;

namespace MarginTrading.CommissionService.Core.Services
{
    public interface ICommissionCalcService
    {
        decimal GetOvernightSwap(IOpenPosition openPosition, decimal swapRate);
        decimal CalculateOrderExecutionCommission(string instrument, string legalEntity, decimal volume);
    }
}