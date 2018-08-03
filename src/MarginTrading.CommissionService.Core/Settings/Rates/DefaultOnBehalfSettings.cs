namespace MarginTrading.CommissionService.Core.Settings.Rates
{
    public class DefaultOnBehalfSettings
    {
        public decimal Commission { get; set; }
        
        public string CommissionAsset { get; set; }
        
        public string DefaultLegalEntity { get; set; } //one legal entity for all the system
    }
}