using System;
using JetBrains.Annotations;
using MarginTrading.CommissionService.Core.Domain.Abstractions;

namespace MarginTrading.CommissionService.Core.Domain.Rates
{
    public class OvernightSwap
    {
        [NotNull] public string AssetPairId { get; }
        
        public decimal RepoSurchargePercent { get; }
        
        public decimal FixRate { get; }
        
        public decimal VariableRateBase { get; }
        
        public decimal VariableRateQuote { get; }

        public OvernightSwap([NotNull] string assetPairId, decimal repoSurchargePercent, decimal fixRate,
            decimal variableRateBase, decimal variableRateQuote)
        {
            AssetPairId = assetPairId ?? throw new ArgumentNullException(nameof(assetPairId));
            RepoSurchargePercent = repoSurchargePercent;
            FixRate = fixRate;
            VariableRateBase = variableRateBase;
            VariableRateQuote = variableRateQuote;
        }
    }
}