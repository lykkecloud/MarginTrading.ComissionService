// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

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