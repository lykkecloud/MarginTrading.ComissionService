namespace MarginTrading.CommissionService.Core.Domain.Rates
{
    public class OnBehalfRate
    {
        public decimal Commission { get; set; }
        
        public string CommissionAsset { get; set; }
    }
}