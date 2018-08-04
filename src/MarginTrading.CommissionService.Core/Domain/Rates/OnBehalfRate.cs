using MarginTrading.CommissionService.Core.Settings.Rates;

namespace MarginTrading.CommissionService.Core.Domain.Rates
{
    public class OnBehalfRate
    {
        public decimal Commission { get; set; }
        
        public string CommissionAsset { get; set; }
        
        public string LegalEntity { get; set; }
        
        public static OnBehalfRate FromDefault(DefaultOnBehalfSettings defaultOnBehalfSettings)
        {
            return new OnBehalfRate
            {
                Commission = defaultOnBehalfSettings.Commission,
                CommissionAsset = defaultOnBehalfSettings.CommissionAsset,
                LegalEntity = defaultOnBehalfSettings.LegalEntity,
            };
        }
    }
}