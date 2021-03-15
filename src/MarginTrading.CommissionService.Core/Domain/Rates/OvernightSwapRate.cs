// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using JetBrains.Annotations;

namespace MarginTrading.CommissionService.Core.Domain.Rates
{
    public class OvernightSwapRate
    {
        [NotNull] public string AssetPairId { get; set; }
        
        public decimal RepoSurchargePercent { get; set; }
        
        [CanBeNull] public string VariableRateBase { get; set; }
        
        [CanBeNull] public string VariableRateQuote { get; set; }
    }
}