// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using JetBrains.Annotations;

namespace MarginTrading.CommissionService.Core.Domain.Rates
{
    public class OnBehalfRate
    {
        public decimal Commission { get; set; }
        
        [NotNull] public string CommissionAsset { get; set; }
        
        [CanBeNull] public string LegalEntity { get; set; }
    }
}