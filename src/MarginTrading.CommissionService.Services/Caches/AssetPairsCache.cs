using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using MoreLinq;
using MarginTrading.CommissionService.Core.Caches;
using MarginTrading.CommissionService.Core.Domain.Abstractions;
using MarginTrading.CommissionService.Core.Exceptions;
using MarginTrading.CommissionService.Core.Services;

namespace MarginTrading.CommissionService.Services.Caches
{
    /// <summary>
    /// Cashes data about assets in the backend app.
    /// </summary>
    /// <remarks>
    /// Note this type is thread-safe, though it has no synchronization.
    /// This is due to the fact that the <see cref="_assetPairs"/> dictionary
    /// is used as read-only: never updated, only reference-assigned.
    /// Their contents are also readonly.
    /// </remarks>
    public class AssetPairsCache : IAssetPairsCache
    {
        private IReadOnlyDictionary<string, IAssetPair> _assetPairs = 
            ImmutableSortedDictionary<string, IAssetPair>.Empty;

        public IAssetPair GetAssetPairById(string assetPairId)
        {
            return _assetPairs.TryGetValue(assetPairId, out var result)
                ? result
                : throw new AssetPairNotFoundException(assetPairId,
                    string.Format("Instrument {0} does not exist in cache", assetPairId));
        }

        public IAssetPair GetAssetPairByIdOrDefault(string assetPairId)
        {
            return _assetPairs.GetValueOrDefault(assetPairId);
        }

        public IEnumerable<IAssetPair> GetAll()
        {
            return _assetPairs.Values;
        }

        public ImmutableHashSet<string> GetAllIds()
        {
            return Enumerable.ToHashSet(_assetPairs.Select(x => x.Key)).ToImmutableHashSet();
        }

        public IAssetPair FindAssetPair(string asset1, string asset2, string legalEntity)
        {
            var key = GetAssetPairKey(asset1, asset2, legalEntity);

            var assetPair = _assetPairs.Values.FirstOrDefault(x => x.BaseAssetId == asset1
                                                                   && x.QuoteAssetId == asset2
                                                                   && x.LegalEntity == legalEntity);
            if (assetPair != null)
                return assetPair;

            throw new InstrumentByAssetsNotFoundException(asset1, asset2,
                string.Format("There is no instrument with assets {0} and {1}", asset1, asset2));
        }

        void IAssetPairsCache.InitPairsCache(Dictionary<string, IAssetPair> instruments)
        {
            _assetPairs = instruments;
        }

        private static (string, string, string) GetAssetPairKey(string asset1, string asset2, string legalEntity)
        {
            return (asset1, asset2, legalEntity);
        }
    }
}