// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using MarginTrading.CommissionService.Core.Domain.CacheModels;

namespace MarginTrading.CommissionService.Core.Caches
{
    public interface IClientProfileSettingsCache
    {
        void AddOrUpdate(ClientProfileSettingsCacheModel clientProfileSettings);
        void Remove(ClientProfileSettingsCacheModel clientProfileSettings);
        ClientProfileSettingsCacheModel GetByIds(string profileId, string assetType);
    }
}