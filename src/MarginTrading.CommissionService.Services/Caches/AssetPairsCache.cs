// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using MoreLinq;
using MarginTrading.CommissionService.Core.Caches;
using MarginTrading.CommissionService.Core.Domain.Abstractions;
using MarginTrading.CommissionService.Core.Exceptions;
using MarginTrading.CommissionService.Core.Services;
using AssetPairKey = System.ValueTuple<string, string, string>;
    
namespace MarginTrading.CommissionService.Services.Caches
{
    /// <summary>
    /// Cashes data about assets in the backend app.
    /// </summary>
    public class AssetPairsCache : IAssetPairsCache
    {
        private IReadOnlyDictionary<string, IAssetPair> _assetPairs = 
            ImmutableSortedDictionary<string, IAssetPair>.Empty;

        private readonly ICachedCalculation<IReadOnlyDictionary<AssetPairKey, IAssetPair>> _assetPairsByAssets;
        
        private readonly object _lock = new object();

        public AssetPairsCache()
        {
            _assetPairsByAssets = GetAssetPairsByAssetsCache();
        }

        public IAssetPair GetAssetPairById(string assetPairId)
        {
            lock (_lock)
            {
                return _assetPairs.TryGetValue(assetPairId, out var result)
                    ? result
                    : throw new AssetPairNotFoundException(assetPairId,
                        $"Instrument {assetPairId} does not exist in cache");
            }
        }

        public IAssetPair GetAssetPairByIdOrDefault(string assetPairId)
        {
            lock (_lock)
            {
                return _assetPairs.GetValueOrDefault(assetPairId);
            }
        }
        
        public IAssetPair FindAssetPair(string asset1, string asset2, string legalEntity)
        {
            lock (_lock)
            {
                var key = GetAssetPairKey(asset1, asset2, legalEntity);

                if (_assetPairsByAssets.Get().TryGetValue(key, out var result))
                    return result;
            }

            throw new InstrumentByAssetsNotFoundException(asset1, asset2, legalEntity);
        }

        public void InitPairsCache(Dictionary<string, IAssetPair> instruments)
        {
            lock (_lock)
            {
                _assetPairs = instruments;
            }
        }

        private ICachedCalculation<IReadOnlyDictionary<AssetPairKey, IAssetPair>> GetAssetPairsByAssetsCache()
        {
            lock (_lock)
            {
                return Calculate.Cached(() => _assetPairs, ReferenceEquals,
                    pairs => pairs.Values.SelectMany(p => new[]
                    {
                        (GetAssetPairKey(p.BaseAssetId, p.QuoteAssetId, p.LegalEntity), p),
                        (GetAssetPairKey(p.QuoteAssetId, p.BaseAssetId, p.LegalEntity), p),
                    }).ToDictionary());
            }
        }

        private static AssetPairKey GetAssetPairKey(string asset1, string asset2, string legalEntity)
        {
            return (asset1, asset2, legalEntity);
        }
    }
}