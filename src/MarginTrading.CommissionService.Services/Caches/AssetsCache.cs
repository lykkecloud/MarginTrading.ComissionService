using System.Collections.Generic;
using System.Threading;
using MarginTrading.CommissionService.Core.Caches;
using MarginTrading.CommissionService.Core.Domain.Abstractions;

namespace MarginTrading.CommissionService.Services.Caches
{
    public class AssetsCache : IAssetsCache
    {
        private const int DefaultAssetAccuracy = 8;
        
        private Dictionary<string, IAsset> _assets = new Dictionary<string, IAsset>();
        private readonly ReaderWriterLockSlim _lockSlim = new ReaderWriterLockSlim();

        public int GetAssetAccuracy(string assetId)
        {
            _lockSlim.EnterReadLock();

            try
            {
                if (_assets.TryGetValue(assetId, out var result))
                    return result.Accuracy;

                return DefaultAssetAccuracy;
            }
            finally
            {
                _lockSlim.ExitReadLock();
            }
        }
        
        internal void Init(Dictionary<string, IAsset> assets)
        {
            _lockSlim.EnterWriteLock();
            try
            {
                _assets = assets;
            }
            finally
            {
                _lockSlim.ExitWriteLock();
            }
        }
    }
}