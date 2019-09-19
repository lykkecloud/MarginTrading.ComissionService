using JetBrains.Annotations;
using MarginTrading.CommissionService.Core.Domain.Abstractions;
using MarginTrading.CommissionService.Core.Settings.Rates;

namespace Migration.Rates
{
    public class OldOnBehalfRate
    {
        public decimal Commission { get; set; }
        
        [NotNull] public string CommissionAsset { get; set; }
        
        [CanBeNull] public string LegalEntity { get; set; }
        
        public static OldOnBehalfRate FromDefault(DefaultOnBehalfSettings defaultOnBehalfSettings,
            string tradingConditionId)
        {
            return new OldOnBehalfRate
            {
                Commission = defaultOnBehalfSettings.Commission,
                CommissionAsset = defaultOnBehalfSettings.CommissionAsset,
                LegalEntity = defaultOnBehalfSettings.LegalEntity,
            };
        }
    }
}