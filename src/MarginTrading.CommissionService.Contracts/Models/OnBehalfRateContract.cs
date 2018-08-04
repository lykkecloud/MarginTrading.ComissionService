namespace Lykke.MarginTrading.CommissionService.Contracts.Models
{
    public class OnBehalfRateContract
    {
        public decimal Commission { get; set; }
        
        public string CommissionAsset { get; set; }
        
        public string LegalEntity { get; set; }
    }
}