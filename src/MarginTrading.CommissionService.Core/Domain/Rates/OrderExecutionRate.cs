﻿using JetBrains.Annotations;
using MarginTrading.CommissionService.Core.Settings.Rates;

namespace MarginTrading.CommissionService.Core.Domain.Rates
{
    public class OrderExecutionRate
    {
        [NotNull] public string TradingConditionId { get; set; }

        [NotNull] public string AssetPairId { get; set; }

        public decimal CommissionCap { get; set; }

        public decimal CommissionFloor { get; set; }

        public decimal CommissionRate { get; set; }

        [NotNull] public string CommissionAsset { get; set; }

        [CanBeNull] public string LegalEntity { get; set; }

        public static OrderExecutionRate FromDefault(DefaultOrderExecutionSettings defaultOrderExecutionSettings,
            string tradingConditionId, string assetPairId)
        {
            return new OrderExecutionRate
            {
                TradingConditionId = tradingConditionId,
                AssetPairId = assetPairId,
                CommissionCap = defaultOrderExecutionSettings.CommissionCap,
                CommissionFloor = defaultOrderExecutionSettings.CommissionFloor,
                CommissionRate = defaultOrderExecutionSettings.CommissionRate,
                CommissionAsset = defaultOrderExecutionSettings.CommissionAsset,
                LegalEntity = defaultOrderExecutionSettings.LegalEntity,
            };
        }
    }
}