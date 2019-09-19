// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

namespace MarginTrading.CommissionService.Core.Domain
{
    public class CostsAndChargesValue
    {
        public CostsAndChargesValue(){}
        
        public CostsAndChargesValue(decimal valueInEur, decimal valueInPercent)
        {
            ValueInEur = valueInEur;
            ValueInPercent = valueInPercent;
        }

        public decimal ValueInEur { get; set; }
        
        public decimal ValueInPercent { get; set; }
    }
}