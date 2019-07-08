// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

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