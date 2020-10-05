// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

namespace MarginTrading.CommissionService.Contracts.Models
{
    public class CostsAndChargesValueContract
    {
        public decimal ValueInEur { get; set; }
        
        public decimal ValueInPercent { get; set; }
    }
}