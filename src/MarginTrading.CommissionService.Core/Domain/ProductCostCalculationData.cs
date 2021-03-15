// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using MarginTrading.CommissionService.Core.Domain.Rates;

namespace MarginTrading.CommissionService.Core.Domain
{
    public class ProductCostCalculationData
    {
        public OvernightSwapRate OvernightSwapRate { get; set; }
        public decimal VariableRateBase { get; set; }
        public decimal VariableRateQuote { get; set; }
    }
}