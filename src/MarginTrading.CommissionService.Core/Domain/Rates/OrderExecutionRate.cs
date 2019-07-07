using JetBrains.Annotations;
using MarginTrading.CommissionService.Core.Domain.Abstractions;
using MarginTrading.CommissionService.Core.Settings.Rates;

namespace MarginTrading.CommissionService.Core.Domain.Rates
{
    public class OrderExecutionRate : IKeyedObject
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

        public string Key => $"{TradingConditionId}_{AssetPairId}";
        public string GetFilterKey() => GetTradingConditionFromKey(Key);

        public static string GetTradingConditionFromKey(string key)
        {
            var keyData = key.Split('_');
            return keyData[0];   
        }

        public static OrderExecutionRate FromDefault(DefaultRateSettings defaults, string key)
        {
            var keyData = key.Split('_');
            return FromDefault(defaults.DefaultOrderExecutionSettings, keyData[0], keyData[1]);
        }
    }
}