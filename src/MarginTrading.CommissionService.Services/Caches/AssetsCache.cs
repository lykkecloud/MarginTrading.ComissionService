// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using MarginTrading.AssetService.Contracts.LegacyAsset;
using MarginTrading.CommissionService.Core.Caches;
using Asset = MarginTrading.CommissionService.Core.Domain.Asset;

namespace MarginTrading.CommissionService.Services.Caches
{
    public class AssetsCache : IAssetsCache
    {
        private int _maxAccuracy = 8;
        
        private IReadOnlyDictionary<string, Asset> _cache = ImmutableSortedDictionary<string, Asset>.Empty;

        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

        public AssetsCache()
        {
            
        }
        
        public void Initialize(Dictionary<string, Asset> data)
        {
            _lock.EnterWriteLock();

            try
            {
                _cache = data;
                _maxAccuracy = _cache.Any()
                    ? _cache.Max(x => x.Value.Accuracy)
                    : 8;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public int GetAccuracy(string id)
        {
            _lock.EnterReadLock();

            try
            {
                return id != null && _cache.TryGetValue(id, out var result) 
                    ? result.Accuracy
                    : _maxAccuracy;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public string GetName(string id)
        {
            _lock.EnterReadLock();

            try
            {
                return id != null && _cache.TryGetValue(id, out var result)
                    ? result.Name
                    : id;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }
        
        public Asset GetAsset(string id)
        {
            _lock.EnterReadLock();

            try
            {
                return id != null && _cache.TryGetValue(id, out var result)
                    ? result
                    : null;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public ClientProfile GetClientProfile(string assetId, string clientProfileId)
        {
            _lock.EnterReadLock();

            try
            {
                return assetId != null && _cache.TryGetValue(assetId, out var result)
                    ? result.AvailableClientProfiles.SingleOrDefault(x => x.Id == clientProfileId)
                    : null;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }
    }
}