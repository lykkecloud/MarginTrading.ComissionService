// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System.Linq;
using Autofac;
using MarginTrading.AssetService.Contracts;
using MarginTrading.AssetService.Contracts.Asset;
using MarginTrading.AssetService.Contracts.AssetPair;
using MarginTrading.AssetService.Contracts.Scheduling;
using MarginTrading.AssetService.Contracts.TradingConditions;
using MarginTrading.CommissionService.Core.Caches;
using MarginTrading.CommissionService.Core.Domain;
using MarginTrading.CommissionService.Core.Domain.Abstractions;
using MarginTrading.CommissionService.Core.Services;

namespace MarginTrading.CommissionService.Services
{
    public class CacheUpdater : ICacheUpdater, IStartable
    {
        private readonly IAssetPairsCache _assetPairsCache;
        private readonly IAssetsCache _assetsCache;
        private readonly IAssetPairsApi _assetPairsApi;
        private readonly IAssetsApi _assetsApi;
        private readonly ITradingInstrumentsApi _tradingInstrumentsApi;
        private readonly ITradingInstrumentsCache _tradingInstrumentsCache;
        private readonly IScheduleSettingsApi _scheduleSettingsApi;
        private readonly ITradingDaysInfoProvider _tradingDaysInfoProvider;
        private readonly IRateSettingsCache _rateSettingsCache;
        private readonly IConvertService _convertService;

        public CacheUpdater(IAssetPairsCache assetPairsCache,
            IAssetsCache assetsCache,
            IAssetPairsApi assetPairsApi,
            IAssetsApi assetsApi,
            ITradingInstrumentsApi tradingInstrumentsApi,
            ITradingInstrumentsCache tradingInstrumentsCache,
            IScheduleSettingsApi scheduleSettingsApi,
            ITradingDaysInfoProvider tradingDaysInfoProvider,
            IRateSettingsCache rateSettingsCache,
            IConvertService convertService)
        {
            _assetPairsCache = assetPairsCache;
            _assetsCache = assetsCache;
            _assetPairsApi = assetPairsApi;
            _assetsApi = assetsApi;
            _tradingInstrumentsApi = tradingInstrumentsApi;
            _tradingInstrumentsCache = tradingInstrumentsCache;
            _scheduleSettingsApi = scheduleSettingsApi;
            _tradingDaysInfoProvider = tradingDaysInfoProvider;
            _rateSettingsCache = rateSettingsCache;
            _convertService = convertService;
        }

        public void Start()
        {
            InitAssets();
            InitAssetPairs();
            InitTradingInstruments();
            InitSchedules();
            InitOrderExecutionRates();
            InitOvernightSwapRates();
        }

        public void InitAssetPairs()
        {
            var pairs = _assetPairsApi.List().GetAwaiter().GetResult()
                .ToDictionary(a => a.Id,
                    s => (IAssetPair) _convertService.Convert<AssetPairContract, AssetPair>(s));
            _assetPairsCache.InitPairsCache(pairs);
        }

        public void InitAssets()
        {
            var assets = _assetsApi.List().GetAwaiter().GetResult()
                .ToDictionary(x => x.Id,
                    s => _convertService.Convert<AssetContract, Asset>(s));
            _assetsCache.Initialize(assets);
        }

        public void InitTradingInstruments()
        {
            var tradingInstruments = _tradingInstrumentsApi.List(string.Empty)
                .GetAwaiter().GetResult()
                .Select(MapTradingInstrument);
            _tradingInstrumentsCache.InitCache(tradingInstruments);
        }

        public void InitSchedules()
        {
            var markets = _assetPairsCache.GetAll().Select(p => p.MarketId).Distinct().ToArray();
            var schedules = _scheduleSettingsApi.GetMarketsInfo(markets)
                .GetAwaiter().GetResult()
                .ToDictionary(k => k.Key, v => MapTradingDayInfo(v.Value));
            _tradingDaysInfoProvider.Initialize(schedules);
        }

        public void InitOrderExecutionRates()
        {
            _rateSettingsCache.RefreshOrderExecutionRates().GetAwaiter().GetResult();
        }

        public void InitOvernightSwapRates()
        {
            _rateSettingsCache.RefreshOvernightSwapRates();
        }

        private static TradingInstrument MapTradingInstrument(TradingInstrumentContract tic) =>
            new TradingInstrument
            {
                TradingConditionId = tic.TradingConditionId,
                Instrument = tic.Instrument,
                HedgeCost = tic.HedgeCost,
                Spread = tic.Spread
            };

        private static TradingDayInfo MapTradingDayInfo(TradingDayInfoContract contract) => new TradingDayInfo
        {
            LastTradingDay = contract.LastTradingDay,
            NextTradingDayStart = contract.NextTradingDayStart
        };
    }
}