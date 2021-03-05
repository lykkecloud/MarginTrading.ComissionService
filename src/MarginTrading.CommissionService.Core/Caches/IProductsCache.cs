// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using MarginTrading.CommissionService.Core.Domain.CacheModels;

namespace MarginTrading.CommissionService.Core.Caches
{
    public interface IProductsCache
    {
        void Initialize();
        List<ProductCacheModel> GetAll();
        ProductCacheModel GetById(string productId);
        void AddOrUpdate(ProductCacheModel product);
        void Remove(ProductCacheModel product);
    }
}