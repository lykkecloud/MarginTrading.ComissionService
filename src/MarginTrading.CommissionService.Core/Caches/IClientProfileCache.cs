// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using MarginTrading.CommissionService.Core.Domain.CacheModels;

namespace MarginTrading.CommissionService.Core.Caches
{
    public interface IClientProfileCache
    {
        void AddOrUpdate(ClientProfileCacheModel clientProfile);
        void Remove(ClientProfileCacheModel clientProfile);
    }
}