// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Linq;
using MoreLinq;
using MarginTrading.CommissionService.Core.Caches;
using MarginTrading.CommissionService.Core.Domain.Abstractions;
using MarginTrading.CommissionService.Core.Exceptions;
using MarginTrading.CommissionService.Core.Services;
using AssetPairKey = System.ValueTuple<string, string, string>;
using System.Threading;

namespace MarginTrading.CommissionService.Services.Caches
{
    /// <summary>
    /// Cashes data about assets in the backend app.
    /// </summary>
    public class AssetPairsCache : IAssetPairsCache
    {
        private Dictionary<string, IAssetPair> _assetPairs = new Dictionary<string, IAssetPair>();

        private readonly ICachedCalculation<IReadOnlyDictionary<AssetPairKey, IAssetPair>> _assetPairsByAssets;

        private readonly ReaderWriterLockSlim _readerWriterLockSlim = new ReaderWriterLockSlim();

        public AssetPairsCache()
        {
            _assetPairsByAssets = GetAssetPairsByAssetsCache();
        }

        public IAssetPair GetAssetPairById(string assetPairId)
        {
            _readerWriterLockSlim.EnterReadLock();

            try
            {
                return _assetPairs.TryGetValue(assetPairId, out var result)
                    ? result
                    : throw new AssetPairNotFoundException(assetPairId,
                        $"Instrument {assetPairId} does not exist in cache");
            }
            finally
            {
                _readerWriterLockSlim.ExitReadLock();
            }
        }

        public IAssetPair[] GetAll()
        {
            _readerWriterLockSlim.EnterReadLock();

            try
            {
                return _assetPairs.Values.ToArray();
            }
            finally
            {
                _readerWriterLockSlim.ExitReadLock();
            }
        }

        public IAssetPair FindAssetPair(string asset1, string asset2, string legalEntity)
        {
            _readerWriterLockSlim.EnterReadLock();

            try
            {
                var key = GetAssetPairKey(asset1, asset2, legalEntity);

                if (_assetPairsByAssets.Get().TryGetValue(key, out var result))
                    return result;

                throw new InstrumentByAssetsNotFoundException(asset1, asset2, legalEntity);
            }
            finally
            {
                _readerWriterLockSlim.ExitReadLock();
            }
        }

        public void InitPairsCache(Dictionary<string, IAssetPair> instruments)
        {
            _readerWriterLockSlim.EnterWriteLock();

            try
            {
                _assetPairs = instruments;
            }
            finally
            {
                _readerWriterLockSlim.ExitWriteLock();
            }
        }

        private ICachedCalculation<IReadOnlyDictionary<AssetPairKey, IAssetPair>> GetAssetPairsByAssetsCache()
        {
            _readerWriterLockSlim.EnterReadLock();

            try
            {
                return Calculate.Cached(() => _assetPairs, ReferenceEquals,
                    pairs => pairs.Values.SelectMany(p => new[]
                    {
                        (GetAssetPairKey(p.BaseAssetId, p.QuoteAssetId, p.LegalEntity), p),
                        (GetAssetPairKey(p.QuoteAssetId, p.BaseAssetId, p.LegalEntity), p),
                    }).ToDictionary());
            }
            finally
            {
                _readerWriterLockSlim.ExitReadLock();
            }
        }

        private static AssetPairKey GetAssetPairKey(string asset1, string asset2, string legalEntity)
        {
            return (asset1, asset2, legalEntity);
        }

        public void AddOrUpdate(IAssetPair assetPair)
        {
            _readerWriterLockSlim.EnterWriteLock();

            try
            {
                _assetPairs[assetPair.Id] = assetPair;
            }
            finally
            {
                _readerWriterLockSlim.ExitWriteLock();
            }
        }

        public void Remove(string assetPairId)
        {
            _readerWriterLockSlim.EnterWriteLock();

            try
            {
                _assetPairs.Remove(assetPairId);
            }
            finally
            {
                _readerWriterLockSlim.ExitWriteLock();
            }
        }
    }
}