using System.Collections.Generic;
using System.Collections.Immutable;
using JetBrains.Annotations;
using MarginTrading.OvernightSwapService.Models.Abstractions;

namespace MarginTrading.OvernightSwapService.Caches
{
    public interface IAssetPairsCache
    {
        IAssetPair GetAssetPairById(string assetPairId);
        /// <summary>
        /// Tries to get an asset pair, if it is not found null is returned.
        /// </summary>
        /// <param name="assetPairId"></param>
        /// <returns></returns>
        [CanBeNull] IAssetPair GetAssetPairByIdOrDefault(string assetPairId);
        IAssetPair FindAssetPair(string asset1, string asset2, string legalEntity);
        
        void InitPairsCache(Dictionary<string, IAssetPair> instruments);
    }
}