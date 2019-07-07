using JetBrains.Annotations;
using MarginTrading.CommissionService.Core.Domain.Abstractions;
using MarginTrading.CommissionService.Core.Settings.Rates;

namespace MarginTrading.CommissionService.Core.Domain.Rates
{
    public class OnBehalfRate : IKeyedObject
    {
        [NotNull] public string TradingConditionId { get; set; }
        
        public decimal Commission { get; set; }
        
        [NotNull] public string CommissionAsset { get; set; }
        
        [CanBeNull] public string LegalEntity { get; set; }
        
        public static OnBehalfRate FromDefault(DefaultOnBehalfSettings defaultOnBehalfSettings,
            string tradingConditionId)
        {
            return new OnBehalfRate
            {
                TradingConditionId = tradingConditionId,
                Commission = defaultOnBehalfSettings.Commission,
                CommissionAsset = defaultOnBehalfSettings.CommissionAsset,
                LegalEntity = defaultOnBehalfSettings.LegalEntity,
            };
        }

        public string Key => TradingConditionId;
        public string GetFilterKey() => TradingConditionId;
    }
}