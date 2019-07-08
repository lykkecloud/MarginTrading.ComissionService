// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

namespace MarginTrading.CommissionService.Core.Settings.Rates
{
    public class DefaultOvernightSwapSettings
    {
        
        public decimal RepoSurchargePercent { get; set; }
        
        public decimal FixRate { get; set; }
        
        public string VariableRateBase { get; set; }
        
        public string VariableRateQuote { get; set; }
    }
}