using JetBrains.Annotations;
using MarginTrading.CommissionService.Core.Domain.Abstractions;
using MarginTrading.CommissionService.Core.Settings.Rates;

namespace Migration.Rates
{
    public class OldOrderExecutionRate
    {
        [NotNull] public string AssetPairId { get; set; }

        public decimal CommissionCap { get; set; }

        public decimal CommissionFloor { get; set; }

        public decimal CommissionRate { get; set; }

        [NotNull] public string CommissionAsset { get; set; }

        [CanBeNull] public string LegalEntity { get; set; }

        public static OldOrderExecutionRate FromDefault(DefaultOrderExecutionSettings defaultOrderExecutionSettings,
            string assetPairId)
        {
            return new OldOrderExecutionRate
            {
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