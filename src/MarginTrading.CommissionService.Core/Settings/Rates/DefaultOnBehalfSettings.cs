// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

namespace MarginTrading.CommissionService.Core.Settings.Rates
{
    public class DefaultOnBehalfSettings
    {
        public decimal Commission { get; set; }
        
        public string CommissionAsset { get; set; }
        
        public string LegalEntity { get; set; }
    }
}