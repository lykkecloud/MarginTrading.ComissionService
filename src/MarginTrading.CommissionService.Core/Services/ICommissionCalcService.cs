using MarginTrading.CommissionService.Core.Domain.Abstractions;

namespace MarginTrading.CommissionService.Core.Services
{
    public interface ICommissionCalcService
    {
        decimal GetOvernightSwap(IOpenPosition openPosition, IAssetPair assetPair);
        decimal CalculateOrderExecutionCommission(string instrument, string legalEntity, decimal volume);
    }
}