using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using MarginTrading.CommissionService.Core.Domain.Abstractions;
using MarginTrading.CommissionService.Core.Services;

namespace MarginTrading.CommissionService.Services
{
    public class AccountAssetsCacheService : IAccountAssetsCacheService
    {
        private Dictionary<(string, string), IAccountAssetPair[]> _accountGroupCache =
            new Dictionary<(string, string), IAccountAssetPair[]>();
        private Dictionary<(string, string, string), IAccountAssetPair> _instrumentsCache =
            new Dictionary<(string, string, string), IAccountAssetPair>();
        private readonly ReaderWriterLockSlim _lockSlim = new ReaderWriterLockSlim();

        public IAccountAssetPair GetAccountAsset(string tradingConditionId, string accountAssetId, string instrument)
        {
            IAccountAssetPair accountAssetPair = null;

            _lockSlim.EnterReadLock();
            try
            {
                var key = GetInstrumentCacheKey(tradingConditionId, accountAssetId, instrument);

                if (_instrumentsCache.ContainsKey(key))
                    accountAssetPair = _instrumentsCache[key];
            }
            finally
            {
                _lockSlim.ExitReadLock();
            }

            if (accountAssetPair == null)
            {
                throw new Exception(string.Format("Can't find AccountAsset for tradingConditionId: {0}, baseAssetId: {1}, instrument: {2}",
                    tradingConditionId, accountAssetId, instrument));
            }

            if (accountAssetPair.LeverageMaintenance < 1)
            {
                throw new Exception(string.Format("LeverageMaintenance &lt; 1 for tradingConditionId: {0}, baseAssetId: {1}, instrument: {2}",
                    tradingConditionId, accountAssetId, instrument));
            }

            if (accountAssetPair.LeverageInit < 1)
            {
                throw new Exception(string.Format("LeverageInit &lt; 1 for tradingConditionId: {0}, baseAssetId: {1}, instrument: {2}", 
                    tradingConditionId, accountAssetId, instrument));
            }

            return accountAssetPair;
        }

        public void InitAccountAssetsCache(List<IAccountAssetPair> accountAssets)
        {
            _lockSlim.EnterWriteLock();
            try
            {
                _accountGroupCache = accountAssets
                    .GroupBy(a => GetAccountGroupCacheKey(a.TradingConditionId, a.BaseAssetId))
                    .ToDictionary(g => g.Key, g => g.ToArray());

                _instrumentsCache = accountAssets
                    .GroupBy(a => GetInstrumentCacheKey(a.TradingConditionId, a.BaseAssetId, a.Instrument))
                    .ToDictionary(g => g.Key, g => g.SingleOrDefault());
            }
            finally
            {
                _lockSlim.ExitWriteLock();
            }
        }

        private (string, string) GetAccountGroupCacheKey(string tradingCondition, string assetId)
        {
            return (tradingCondition, assetId);
        }

        private (string, string, string) GetInstrumentCacheKey(string tradingCondition, string assetId, string instrument)
        {
            return (tradingCondition, assetId, instrument);
        }
    }
}