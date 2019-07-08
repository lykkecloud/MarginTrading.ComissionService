// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using MarginTrading.CommissionService.Core.Domain.Rates;

namespace MarginTrading.CommissionService.Core.Settings.Rates
{
    public class DefaultOrderExecutionSettings
    {
        public decimal CommissionCap { get; set; }
        
        public decimal CommissionFloor { get; set; }
        
        public decimal CommissionRate { get; set; }
        
        public string CommissionAsset { get; set; }
        
        public string LegalEntity { get; set; }
    }
}