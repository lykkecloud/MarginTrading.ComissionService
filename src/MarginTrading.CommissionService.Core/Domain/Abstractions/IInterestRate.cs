using System;

namespace MarginTrading.CommissionService.Core.Domain.Abstractions
{
    public interface IInterestRate
    {
        string AssetPairId { get; }
        
        decimal Rate { get; }
        
        DateTime Timestamp { get; }
    }
}