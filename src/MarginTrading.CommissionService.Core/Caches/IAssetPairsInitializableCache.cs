using System.Collections.Generic;
using MarginTrading.CommissionService.Core.Domain.Abstractions;

namespace MarginTrading.CommissionService.Core.Caches
{
    public interface IAssetPairsInitializableCache : IAssetPairsCache
    {
        void InitPairsCache(Dictionary<string, IAssetPair> instruments);
    }
}