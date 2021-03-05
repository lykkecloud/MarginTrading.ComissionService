// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

namespace MarginTrading.CommissionService.Core.Domain.CacheModels
{
    public class ProductCacheModel
    {
        public string ProductId { get; set; }
        public string IsinLong { get; set; }
        public string IsinShort { get; set; }
    }
}