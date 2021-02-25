// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

namespace MarginTrading.CommissionService.Core.Domain
{
    public class CommissionHistory
    {
        public string OrderId { get; set; }
        public decimal? Commission { get; set; }
        public decimal? ProductCost { get; set; }
    }
}