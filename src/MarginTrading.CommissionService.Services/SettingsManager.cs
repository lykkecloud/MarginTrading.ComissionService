﻿// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Common.Log;
using JetBrains.Annotations;
using MarginTrading.AssetService.Contracts;
using MarginTrading.AssetService.Contracts.Asset;
using MarginTrading.AssetService.Contracts.AssetPair;
using MarginTrading.AssetService.Contracts.Enums;
using MarginTrading.AssetService.Contracts.Messages;
using MarginTrading.AssetService.Contracts.Scheduling;
using MarginTrading.AssetService.Contracts.TradingConditions;
using MarginTrading.CommissionService.Core.Caches;
using MarginTrading.CommissionService.Core.Domain;
using MarginTrading.CommissionService.Core.Domain.Abstractions;
using MarginTrading.CommissionService.Core.Services;
using Newtonsoft.Json;

namespace MarginTrading.CommissionService.Services
{
    [UsedImplicitly]
    public class SettingsManager : IStartable, ISettingsManager
    {
        private static readonly object Lock = new object();

        private readonly IAssetPairsCache _assetPairsCache;
        private readonly IAssetsCache _assetsCache;
        private readonly ITradingInstrumentsCache _tradingInstrumentsCache;
        private readonly IAssetPairsApi _assetPairs;
        private readonly IAssetsApi _assetsApi;
        private readonly ITradingInstrumentsApi _tradingInstrumentsApi;
        private readonly IConvertService _convertService;
        private readonly ILog _log;
        private readonly ITradingDaysInfoProvider _tradingDaysInfoProvider;
        private readonly IScheduleSettingsApi _scheduleSettingsApi;

        public SettingsManager(
            IAssetPairsCache assetPairsCache,
            IAssetsCache assetsCache, 
            ITradingInstrumentsCache tradingInstrumentsCache,
            IAssetPairsApi assetPairs,
            IAssetsApi assetsApi, 
            ITradingInstrumentsApi tradingInstrumentsApi,
            IConvertService convertService, 
            ILog log,
            ITradingDaysInfoProvider tradingDaysInfoProvider,
            IScheduleSettingsApi scheduleSettingsApi)
        {
            _assetPairsCache = assetPairsCache;
            _assetsCache = assetsCache;
            _tradingInstrumentsCache = tradingInstrumentsCache;
            _assetPairs = assetPairs;
            _assetsApi = assetsApi;
            _tradingInstrumentsApi = tradingInstrumentsApi;
            _convertService = convertService;
            _log = log;
            _tradingDaysInfoProvider = tradingDaysInfoProvider;
            _scheduleSettingsApi = scheduleSettingsApi;
        }

        public void Start()
        {
            InitAssetPairs();
            InitAssets();
            InitTradingInstruments();
            InitSchedules();
        }

        private void InitAssetPairs()
        {
            lock (Lock)
            {
                var pairs = _assetPairs.List().GetAwaiter().GetResult()
                    .ToDictionary(a => a.Id,
                        s => (IAssetPair) _convertService.Convert<AssetPairContract, AssetPair>(s));
                _assetPairsCache.InitPairsCache(pairs);
            }
        }
        
        private void InitAssets()
        {
            lock (Lock)
            {
                var assets = _assetsApi.List().GetAwaiter().GetResult()
                    .ToDictionary(x => x.Id,
                        s => _convertService.Convert<AssetContract, Asset>(s));
                _assetsCache.Initialize(assets);
            }
        }
        
        private void InitTradingInstruments()
        {
            lock (Lock)
            {
                var tradingInstruments = _tradingInstrumentsApi.List(string.Empty)
                    .GetAwaiter().GetResult()
                    .Select(Map);
                _tradingInstrumentsCache.InitCache(tradingInstruments);
            }
        }
        
        private void InitSchedules()
        {
            lock (Lock)
            {
                var markets = _assetPairsCache.GetAll().Select(p => p.MarketId).Distinct().ToArray();
                var schedules = _scheduleSettingsApi.GetMarketsInfo(markets)
                    .GetAwaiter().GetResult()
                    .ToDictionary(k => k.Key, v => Map(v.Value));
                _tradingDaysInfoProvider.Initialize(schedules);
            }
        }
        
        public async Task UpdateTradingInstrumentsCacheAsync(string id = null)
        {
            _log.WriteInfo(nameof(SettingsManager), nameof(UpdateTradingInstrumentsCacheAsync), 
                $"Started updating trading instruments cache");

            var count = 0;
            if (string.IsNullOrEmpty(id))
            {
                var instruments = (await _tradingInstrumentsApi.List(string.Empty))?
                    .Select(Map)
                    .ToDictionary(x => x.GetKey());

                if (instruments != null)
                {
                    _tradingInstrumentsCache.InitCache(instruments.Values);
   
                    count = instruments.Count;
                }
            }
            else
            {
                var ids = JsonConvert.DeserializeObject<TradingInstrumentContract>(id);
                var instrumentContract = await _tradingInstrumentsApi.Get(ids.TradingConditionId, ids.Instrument);
                
                if (instrumentContract != null)
                {
                    var newInstrument = Map(instrumentContract);
                    
                    _tradingInstrumentsCache.Update(newInstrument);
                    
                    count = 1;
                }
                else
                {
                    _tradingInstrumentsCache.Remove(ids.TradingConditionId, ids.Instrument);
                }
            }

            _log.WriteInfo(nameof(SettingsManager), nameof(UpdateTradingInstrumentsCacheAsync), 
                $"Finished updating trading instruments cache with count: {count}.");
        }

        public async Task HandleSettingsChanged(SettingsChangedEvent evt)
        {
            if (evt.SettingsType == SettingsTypeContract.Asset)
            {
                InitAssets();
            }
            else if (evt.SettingsType == SettingsTypeContract.TradingInstrument)
            {
                await UpdateTradingInstrumentsCacheAsync(evt.ChangedEntityId);
            }
            else if (evt.SettingsType == SettingsTypeContract.ScheduleSettings)
            {
                InitSchedules();
            }
        }

        private static TradingInstrument Map(TradingInstrumentContract tic) =>
            new TradingInstrument
            {
                TradingConditionId = tic.TradingConditionId,
                Instrument = tic.Instrument,
                HedgeCost = tic.HedgeCost,
                Spread = tic.Spread
            };

        private static TradingDayInfo Map(TradingDayInfoContract contract) => new TradingDayInfo
        {
            LastTradingDay = contract.LastTradingDay,
            NextTradingDayStart = contract.NextTradingDayStart
        };
    }
}