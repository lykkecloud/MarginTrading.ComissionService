// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using AutoMapper;
using Common.Log;
using MarginTrading.CommissionService.Core.Domain;
using MarginTrading.CommissionService.Core.Services;
using MarginTrading.SettingsService.Contracts;
using MarginTrading.SettingsService.Contracts.TradingConditions;
using Newtonsoft.Json;

namespace MarginTrading.CommissionService.Services.Caches
{
    public class TradingInstrumentsCache : ITradingInstrumentsCache
    {
        private Dictionary<(string, string), TradingInstrument> _instrumentsCache =
            new Dictionary<(string, string), TradingInstrument>();

        private readonly ReaderWriterLockSlim _lockSlim = new ReaderWriterLockSlim();

        public TradingInstrument Get(string tradingConditionId, string instrument)
        {
            TradingInstrument accountAssetPair = null;

            _lockSlim.EnterReadLock();
            try
            {
                var key = GetInstrumentCacheKey(tradingConditionId, instrument);

                if (_instrumentsCache.ContainsKey(key))
                    accountAssetPair = _instrumentsCache[key];
            }
            finally
            {
                _lockSlim.ExitReadLock();
            }

            if (accountAssetPair == null)
            {
                throw new Exception(
                    $"Can't find AccountAsset for tradingConditionId: {tradingConditionId}, instrument: {instrument}");
            }

            return accountAssetPair;
        }

        public void InitCache(IEnumerable<TradingInstrument> tradingInstruments)
        {
            _lockSlim.EnterWriteLock();
            try
            {
                _instrumentsCache = tradingInstruments
                    .GroupBy(a => GetInstrumentCacheKey(a.TradingConditionId, a.Instrument))
                    .ToDictionary(g => g.Key, g => g.SingleOrDefault());
            }
            finally
            {
                _lockSlim.ExitWriteLock();
            }
        }
        
        public void Update(TradingInstrument tradingInstrument)
        {
            _lockSlim.EnterWriteLock();
            try
            {
                _instrumentsCache[tradingInstrument.GetKey()] = tradingInstrument;
            }
            finally
            {
                _lockSlim.ExitWriteLock();
            }
        }
        
        public void Remove(string tradingConditionId, string instrument)
        {
            _lockSlim.EnterWriteLock();
            try
            {
                var key = GetInstrumentCacheKey(tradingConditionId, instrument);

                _instrumentsCache.Remove(key);
            }
            finally
            {
                _lockSlim.ExitWriteLock();
            }
        }
        
        private static (string, string) GetInstrumentCacheKey(string tradingCondition, string instrument) =>
            (tradingCondition, instrument);
    }
}