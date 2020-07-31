// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using JetBrains.Annotations;

namespace MarginTrading.CommissionService.Core.Domain.Rates
{
    public class OrderExecutionRate
    {
        [NotNull] public string AssetPairId { get; set; }
        
        public decimal CommissionCap { get; set; }
        
        public decimal CommissionFloor { get; set; }
        
        public decimal CommissionRate { get; set; }
        
        [NotNull] public string CommissionAsset { get; set; }
        
        [CanBeNull] public string LegalEntity { get; set; }
    }
}