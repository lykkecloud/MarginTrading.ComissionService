// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using MarginTrading.CommissionService.Core.Domain.Abstractions;
using JetBrains.Annotations;

namespace MarginTrading.CommissionService.Core.Caches
{
    public interface IAssetPairsCache
    {
        IAssetPair GetAssetPairById(string assetPairId);
        
        IAssetPair[] GetAll();
        
        IAssetPair FindAssetPair(string asset1, string asset2, string legalEntity);

        void InitPairsCache(Dictionary<string, IAssetPair> instruments);
    }
}