using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using MarginTrading.CommissionService.Core.Caches;
using MarginTrading.CommissionService.Core.Domain;

namespace MarginTrading.CommissionService.Services.Caches
{
    public class AssetsCache : IAssetsCache
    {
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
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public Asset Get(string id)
        {
            _lock.EnterReadLock();

            try
            {
                return _cache.TryGetValue(id, out var result) 
                    ? result 
                    : throw new Exception($"Asset {id} does not exist in cache");
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }
    }
}