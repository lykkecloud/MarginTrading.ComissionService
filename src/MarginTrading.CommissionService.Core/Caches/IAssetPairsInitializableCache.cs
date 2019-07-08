// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using MarginTrading.CommissionService.Core.Domain.Abstractions;

namespace MarginTrading.CommissionService.Core.Caches
{
    public interface IAssetPairsInitializableCache : IAssetPairsCache
    {
        void InitPairsCache(Dictionary<string, IAssetPair> instruments);
    }
}