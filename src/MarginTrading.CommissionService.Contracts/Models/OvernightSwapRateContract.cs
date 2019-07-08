// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using JetBrains.Annotations;

namespace Lykke.MarginTrading.CommissionService.Contracts.Models
{
    public class OvernightSwapRateContract
    {
        [NotNull] public string AssetPairId { get; set; }
        
        public decimal RepoSurchargePercent { get; set; }
        
        public decimal FixRate { get; set; }
        
        public string VariableRateBase { get; set; }
        
        public string VariableRateQuote { get; set; }
    }
}