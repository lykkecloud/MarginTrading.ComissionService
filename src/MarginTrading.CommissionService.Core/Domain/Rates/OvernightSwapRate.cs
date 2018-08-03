using System;
using JetBrains.Annotations;
using MarginTrading.CommissionService.Core.Domain.Abstractions;

namespace MarginTrading.CommissionService.Core.Domain.Rates
{
    public class OvernightSwapRate
    {
        [NotNull] public string AssetPairId { get; set; }
        
        public decimal RepoSurchargePercent { get; set; }
        
        public decimal FixRate { get; set; }
        
        public decimal VariableRateBase { get; set; }
        
        public decimal VariableRateQuote { get; set; }
    }
}