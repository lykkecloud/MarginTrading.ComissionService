using JetBrains.Annotations;

namespace Lykke.MarginTrading.CommissionService.Contracts.Models
{
    public class OnBehalfRateContract
    {
        public decimal Commission { get; set; }
        
        [NotNull] public string CommissionAsset { get; set; }
        
        [CanBeNull] public string LegalEntity { get; set; }
    }
}