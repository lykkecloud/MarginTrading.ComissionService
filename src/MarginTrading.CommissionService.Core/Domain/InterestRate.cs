// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using MarginTrading.CommissionService.Core.Domain.Abstractions;

namespace MarginTrading.CommissionService.Core.Domain
{
    public class InterestRate : IInterestRate
    {
        public string AssetPairId { get; set; }
        
        public decimal Rate { get; set; }
        
        public DateTime Timestamp { get; set; }
    }
}