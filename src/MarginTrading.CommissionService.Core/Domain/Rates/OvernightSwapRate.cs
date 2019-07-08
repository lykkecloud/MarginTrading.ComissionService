// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using JetBrains.Annotations;
using MarginTrading.CommissionService.Core.Domain.Abstractions;
using MarginTrading.CommissionService.Core.Settings.Rates;

namespace MarginTrading.CommissionService.Core.Domain.Rates
{
    public class OvernightSwapRate
    {
        [NotNull] public string AssetPairId { get; set; }
        
        public decimal RepoSurchargePercent { get; set; }
        
        public decimal FixRate { get; set; }
        
        [CanBeNull] public string VariableRateBase { get; set; }
        
        [CanBeNull] public string VariableRateQuote { get; set; }

        public static OvernightSwapRate FromDefault(DefaultOvernightSwapSettings defaultOvernightSwapSettings,
            string assetPairId)
        {
            return new OvernightSwapRate
            {
                AssetPairId = assetPairId,
                RepoSurchargePercent = defaultOvernightSwapSettings.RepoSurchargePercent,
                FixRate = defaultOvernightSwapSettings.FixRate,
                VariableRateBase = defaultOvernightSwapSettings.VariableRateBase,
                VariableRateQuote = defaultOvernightSwapSettings.VariableRateQuote,
            };
        }
    }
}